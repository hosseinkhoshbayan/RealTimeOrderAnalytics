# 🚀 Real-Time Order Analytics Platform

A distributed microservices system for real-time order processing and analytics

## 📋 System Architecture

```
┌─────────────┐      ┌──────────────┐      ┌─────────────────────┐      ┌──────────┐
│  Order API  │─────▶│   RabbitMQ   │─────▶│ Analytics Processor │─────▶│ MongoDB  │
│   (C#/.NET) │      │    (Queue)   │      │   (Node.js + API)   │      │ Database │
└─────────────┘      └──────────────┘      └─────────────────────┘      └──────────┘
```

## 🛠️ Tech Stack

- **Order API**: .NET 10 (C#)
- **Analytics Processor**: Node.js 20 + Express
- **Message Broker**: RabbitMQ
- **Database**: MongoDB
- **Containerization**: Docker & Docker Compose

## 🎯 Features

- ✅ Microservices Architecture
- ✅ RESTful APIs with Swagger Documentation
- ✅ Input Validation & Error Handling
- ✅ Asynchronous Communication via RabbitMQ
- ✅ Persistent Data Storage with MongoDB
- ✅ Health Checks for all services
- ✅ Fully Containerized with Docker
- ✅ Real-Time Processing
- ✅ Scalable & Decoupled Design
- ✅ Production-Ready Setup

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- Git

### Installation & Running

1. **Clone the repository:**
```bash
git clone https://github.com/YOUR_USERNAME/RealTimeOrderAnalytics.git
cd RealTimeOrderAnalytics
```

2. **Start the entire system:**
```bash
docker-compose up --build
```

Wait for all services to start (approximately 30 seconds)

3. **Test the system:**
```bash
# Create an order (RESTful endpoint)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "OrderId": "ORD-001",
    "ProductId": "PROD-123",
    "Quantity": 5
  }'

# View Order API stats
curl http://localhost:8080/api/orders/stats

# View all orders in Analytics
curl http://localhost:3000/api/orders

# View analytics statistics
curl http://localhost:3000/api/stats

# OR use the automated test script
chmod +x test-system.sh
./test-system.sh
```

You should see the order being processed in the Analytics Processor logs!

## 📊 Monitoring & Management

- **Order API**: http://localhost:8080
- **Order API Swagger**: http://localhost:8080/swagger
- **Order API Health**: http://localhost:8080/health
- **Analytics API**: http://localhost:3000/api/orders
- **Analytics Health**: http://localhost:3000/health
- **RabbitMQ Management UI**: http://localhost:15672
  - Username: `guest`
  - Password: `guest`
- **MongoDB**: mongodb://localhost:27017

## 📂 Project Structure

```
RealTimeOrderAnalytics/
├── docker-compose.yml
├── test-system.sh
├── API_DOCUMENTATION.md
├── DEVELOPMENT_GUIDE.md
├── CHANGELOG.md
├── .gitignore
├── README.md
├── OrderApi/                        # C# Microservice
│   ├── Models/
│   │   └── Order.cs                 # Data models & DTOs
│   ├── Services/
│   │   ├── IMessagePublisher.cs     # Publisher interface
│   │   ├── RabbitMqPublisher.cs     # RabbitMQ implementation
│   │   ├── OrderService.cs          # Order business logic
│   │   └── HealthCheckService.cs    # Health monitoring
│   ├── Validators/
│   │   └── OrderValidator.cs        # Business rules validation
│   ├── Program.cs
│   ├── appsettings.json
│   ├── OrderApi.csproj
│   ├── Dockerfile
│   └── README.md
├── OrderApi.Tests/                  # Unit Tests
│   ├── OrderValidatorTests.cs
│   └── OrderApi.Tests.csproj
└── AnalyticsProcessor/              # Node.js Microservice
    ├── index.js
    ├── package.json
    ├── Dockerfile
    └── README.md
```

## 🔧 Development

### Running Individual Services

**Order API (C#):**
```bash
cd OrderApi
dotnet restore
dotnet run
```

**Analytics Processor (Node.js):**
```bash
cd AnalyticsProcessor
npm install
npm start
```

**RabbitMQ:**
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

## 🧪 Testing

### Automated Testing
```bash
chmod +x test-system.sh
./test-system.sh
```

### Manual Testing

**Create Orders:**
```bash
# Valid order
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-001", "ProductId": "PROD-123", "Quantity": 5}'

# Invalid order (will return 400)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-002", "ProductId": "PROD-456", "Quantity": 0}'
```

**Check Health:**
```bash
curl http://localhost:8080/health
curl http://localhost:3000/health
```

**View All Orders:**
```bash
curl http://localhost:3000/api/orders
```

**Get Specific Order:**
```bash
curl http://localhost:3000/api/orders/ORD-001
```

**View Statistics:**
```bash
# Order API stats
curl http://localhost:8080/api/orders/stats

# Analytics stats
curl http://localhost:3000/api/stats
```

**Access MongoDB Directly:**
```bash
docker exec -it mongodb mongosh
use analytics
db.orders.find().pretty()
```

See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for complete API reference.

## 🏗️ Architecture Decisions

### Why Microservices?
- **Separation of Concerns**: Each service has a single responsibility
- **Technology Flexibility**: Use the best tool for each job
- **Independent Scaling**: Scale services based on demand
- **Fault Isolation**: One service failure doesn't bring down the entire system

### Why RabbitMQ?
- **Decoupling**: Services don't need to know about each other
- **Reliability**: Messages are persisted and guaranteed delivery
- **Scalability**: Can handle thousands of messages per second
- **Asynchronous Processing**: Non-blocking operations

### Why Docker?
- **Consistency**: Same environment everywhere (dev, staging, production)
- **Portability**: Run anywhere Docker is installed
- **Easy Deployment**: Single command to start entire system
- **Resource Efficiency**: Lightweight compared to VMs

## 🔄 How It Works

1. **Client** sends a POST request to Order API (`POST /order`) with order details
2. **Order API** validates the order and publishes it to RabbitMQ queue (`order_placed`)
3. **RabbitMQ** stores the message reliably in the queue
4. **Analytics Processor** consumes the message from the queue
5. **Analytics Processor** saves the order to MongoDB database
6. **Analytics Processor** provides REST API to query orders and statistics
7. **Client** can retrieve orders via Analytics API (`GET /api/orders`)

## 🚀 Scaling

Scale the Analytics Processor to handle more load:
```bash
docker-compose up --scale analytics-processor=3
```

## 🛑 Stopping the System

```bash
# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

## 📚 Learning Resources

- [Microservices Architecture](https://microservices.io/)
- [RabbitMQ Tutorial](https://www.rabbitmq.com/getstarted.html)
- [Docker Documentation](https://docs.docker.com/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)
- [Node.js Best Practices](https://github.com/goldbergyoni/nodebestpractices)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📝 License

MIT License - feel free to use this project for learning and portfolio purposes.

## 👨‍💻 Author

Built with ❤️ to demonstrate microservices architecture skills

---

**⭐ If you found this project helpful, please give it a star!**