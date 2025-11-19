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
- **Analytics Processor**: Node.js 20
- **Message Broker**: RabbitMQ
- **Containerization**: Docker & Docker Compose

## 🎯 Features

- ✅ Microservices Architecture
- ✅ Asynchronous Communication via RabbitMQ
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
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{
    "OrderId": "ORD-001",
    "ProductId": "PROD-123",
    "Quantity": 5
  }'
```

You should see the order being processed in the Analytics Processor logs!

## 📊 Monitoring & Management

- **Order API**: http://localhost:8080
- **RabbitMQ Management UI**: http://localhost:15672
  - Username: `guest`
  - Password: `guest`

## 📂 Project Structure

```
RealTimeOrderAnalytics/
├── docker-compose.yml
├── OrderApi/
│   ├── Program.cs
│   ├── OrderApi.csproj
│   └── Dockerfile
└── AnalyticsProcessor/
    ├── index.js
    ├── package.json
    └── Dockerfile
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

### Send Multiple Orders
```bash
# Order 1
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-001", "ProductId": "PROD-123", "Quantity": 5}'

# Order 2
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-002", "ProductId": "PROD-456", "Quantity": 3}'
```

### View Logs
```bash
# Order API logs
docker logs order-api

# Analytics Processor logs (real-time)
docker logs analytics-processor -f

# RabbitMQ logs
docker logs rabbitmq
```

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

1. **Client** sends a POST request to Order API with order details
2. **Order API** validates and publishes the order to RabbitMQ queue
3. **RabbitMQ** stores the message reliably
4. **Analytics Processor** consumes the message from the queue
5. **Analytics Processor** processes and logs the order data

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
