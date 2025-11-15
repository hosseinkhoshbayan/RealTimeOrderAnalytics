const amqp = require('amqplib');

const RABBITMQ_HOST = process.env.RABBITMQ_HOST || 'localhost';
const QUEUE_NAME = 'order_placed';

async function startProcessor() {
    try {
        // اتصال به RabbitMQ
        console.log(`📡 در حال اتصال به RabbitMQ در ${RABBITMQ_HOST}...`);
        const connection = await amqp.connect(`amqp://${RABBITMQ_HOST}`);
        const channel = await connection.createChannel();

        // اطمینان از وجود صف
        await channel.assertQueue(QUEUE_NAME, { durable: true });
        
        console.log(`✅ متصل شد! در انتظار پیام‌ها در صف "${QUEUE_NAME}"...`);

        // مصرف پیام‌ها
        channel.consume(QUEUE_NAME, (msg) => {
            if (msg !== null) {
                const order = JSON.parse(msg.content.toString());
                
                console.log('\n📦 ===== سفارش جدید دریافت شد =====');
                console.log(`🆔 Order ID: ${order.OrderId}`);
                console.log(`📦 Product ID: ${order.ProductId}`);
                console.log(`🔢 Quantity: ${order.Quantity}`);
                console.log(`⏰ زمان پردازش: ${new Date().toLocaleString('fa-IR')}`);
                console.log('=====================================\n');

                // تایید پردازش پیام
                channel.ack(msg);
            }
        });

        // مدیریت خاموش شدن
        process.on('SIGINT', async () => {
            console.log('\n🛑 در حال بستن اتصال...');
            await channel.close();
            await connection.close();
            process.exit(0);
        });

    } catch (error) {
        console.error('❌ خطا در اتصال به RabbitMQ:', error.message);
        setTimeout(startProcessor, 5000); // تلاش مجدد بعد از 5 ثانیه
    }
}

startProcessor();