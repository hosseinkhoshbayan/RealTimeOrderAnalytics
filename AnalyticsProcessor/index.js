const amqp = require('amqplib');
const express = require('express');
const { MongoClient } = require('mongodb');

const RABBITMQ_HOST = process.env.RABBITMQ_HOST || 'localhost';
const MONGODB_URI = process.env.MONGODB_URI || 'mongodb://mongodb:27017';
const QUEUE_NAME = 'order_placed';
const PORT = process.env.PORT || 3000;

let db;
let ordersCollection;

//Connect to MongoDB
async function connectToMongoDB() {
    try {
        console.log(`📊 Connect to MongoDB...`);
        const client = new MongoClient(MONGODB_URI);
        await client.connect();
        db = client.db('analytics');
        ordersCollection = db.collection('orders');
        
       // Create index to improve performance
        await ordersCollection.createIndex({ orderId: 1 });
        await ordersCollection.createIndex({ createdAt: -1 });
        
        console.log(`✅ Success Connect to MongoDB!`);
    } catch (error) {
        console.error('❌ Failed Connect to MongoDB:', error.message);
        setTimeout(connectToMongoDB, 5000);
    }
}

// Save order in MongoDB
async function saveOrder(order) {
    try {
        const orderDocument = {
            orderId: order.OrderId,
            productId: order.ProductId,
            quantity: order.Quantity,
            createdAt: new Date(),
            processedAt: new Date(),
            status: 'processed'
        };
        
        const result = await ordersCollection.insertOne(orderDocument);
        console.log(`💾 Order stored in MongoDB with ID: ${result.insertedId}`);
        return result;
    } catch (error) {
        console.error('❌ Error saving order:', error.message);
        throw error;
    }
}

// Processing RabbitMQ messages
async function startRabbitMQConsumer() {
    try {
        console.log(`📡 Connecting to RabbitMQ on ${RABBITMQ_HOST}...`);
        const connection = await amqp.connect(`amqp://${RABBITMQ_HOST}`);
        const channel = await connection.createChannel();

        await channel.assertQueue(QUEUE_NAME, { durable: true });
        
        console.log(`✅ RabbitMQ Connected! Waiting for messages in queue "${QUEUE_NAME}"...`);

        channel.consume(QUEUE_NAME, async (msg) => {
            if (msg !== null) {
                try {
                    const order = JSON.parse(msg.content.toString());
                    
                    console.log('\n📦 ===== New order received. =====');
                    console.log(`🆔 Order ID: ${order.OrderId}`);
                    console.log(`📦 Product ID: ${order.ProductId}`);
                    console.log(`🔢 Quantity: ${order.Quantity}`);
                    console.log(`⏰ Receiving time: ${new Date().toISOString()}`);
                    
                    // Save to MongoDB
                    await saveOrder(order);
                    
                    console.log('=====================================\n');
                    
                    // Confirm message processing
                    channel.ack(msg);
                } catch (error) {
                    console.error('❌ Error processing message:', error.message);
                   // In case of error, we do not reject the message to be processed again
                    channel.nack(msg, false, true);
                }
            }
        });

       // Shutdown management
        process.on('SIGINT', async () => {
            console.log('\n🛑 Closing RabbitMQ connection...');
            await channel.close();
            await connection.close();
        });

    } catch (error) {
        console.error('❌ Error connecting to RabbitMQ:', error.message);
        setTimeout(startRabbitMQConsumer, 5000);
    }
}

// Setting up Express API
async function startAPI() {
    const app = express();
    app.use(express.json());

    // Health Check
    app.get('/health', (req, res) => {
        res.json({ 
            status: 'healthy',
            mongodb: db ? 'connected' : 'disconnected',
            timestamp: new Date().toISOString()
        });
    });

   // Get a list of all orders
    app.get('/api/orders', async (req, res) => {
        try {
            const page = parseInt(req.query.page) || 1;
            const limit = parseInt(req.query.limit) || 10;
            const skip = (page - 1) * limit;

            const orders = await ordersCollection
                .find({})
                .sort({ createdAt: -1 })
                .skip(skip)
                .limit(limit)
                .toArray();

            const total = await ordersCollection.countDocuments({});

            res.json({
                success: true,
                data: orders,
                pagination: {
                    page,
                    limit,
                    total,
                    totalPages: Math.ceil(total / limit)
                }
            });
        } catch (error) {
            console.error('❌ Error in receiving orders:', error.message);
            res.status(500).json({ 
                success: false, 
                error: 'Error receiving orders' 
            });
        }
    });

  // Get an order based on OrderId
    app.get('/api/orders/:orderId', async (req, res) => {
        try {
            const order = await ordersCollection.findOne({ 
                orderId: req.params.orderId 
            });

            if (!order) {
                return res.status(404).json({ 
                    success: false, 
                    error: 'Orther not found' 
                });
            }

            res.json({
                success: true,
                data: order
            });
        } catch (error) {
            console.error('❌ Error in receiving order:', error.message);
            res.status(500).json({ 
                success: false, 
                error: 'Error in receiving order' 
            });
        }
    });

  // Overall order statistics
    app.get('/api/stats', async (req, res) => {
        try {
            const totalOrders = await ordersCollection.countDocuments({});
            const totalQuantity = await ordersCollection.aggregate([
                { $group: { _id: null, total: { $sum: '$quantity' } } }
            ]).toArray();

            const recentOrders = await ordersCollection
                .find({})
                .sort({ createdAt: -1 })
                .limit(5)
                .toArray();

            res.json({
                success: true,
                data: {
                    totalOrders,
                    totalQuantity: totalQuantity[0]?.total || 0,
                    recentOrders
                }
            });
        } catch (error) {
            console.error('❌ Error retrieving statistics:', error.message);
            res.status(500).json({ 
                success: false, 
                error: 'Error retrieving statistics' 
            });
        }
    });

   // Delete an order (for testing)
    app.delete('/api/orders/:orderId', async (req, res) => {
        try {
            const result = await ordersCollection.deleteOne({ 
                orderId: req.params.orderId 
            });

            if (result.deletedCount === 0) {
                return res.status(404).json({ 
                    success: false, 
                    error: 'Orther not found' 
                });
            }

            res.json({
                success: true,
                message: 'Success deleting orther'
            });
        } catch (error) {
            console.error('❌ Error deleting order:', error.message);
            res.status(500).json({ 
                success: false, 
                error: 'Error deleting order' 
            });
        }
    });

    app.listen(PORT, () => {
        console.log(`🚀 API Server Running on port ${PORT}`);
        console.log(`📍 http://localhost:${PORT}/api/orders`);
    });
}

// System startup
async function start() {
    await connectToMongoDB();
    await startAPI();
    await startRabbitMQConsumer();
}

start();