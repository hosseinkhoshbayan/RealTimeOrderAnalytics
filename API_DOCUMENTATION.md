# 📚 API Documentation

## Base URLs
- **Order API**: `http://localhost:8080`
- **Analytics API**: `http://localhost:3000`

---

# 📚 API Documentation

## Base URLs
- **Order API**: `http://localhost:8080`
- **Analytics API**: `http://localhost:3000`
- **Swagger UI (Order API)**: `http://localhost:8080/swagger`

---

## 📦 Order API Endpoints

### 1. Get API Info
Get basic information about the Order API service.

**Endpoint**: `GET /`

**Response**: `200 OK`
```json
{
  "service": "Order API",
  "version": "1.0.0",
  "status": "running",
  "endpoints": [
    "GET /health - Health check",
    "POST /api/orders - Create new order",
    "GET /api/orders/stats - Get order statistics"
  ]
}
```

**Example**:
```bash
curl http://localhost:8080/
```

---

### 2. Health Check
Check if the Order API service and RabbitMQ connection are healthy.

**Endpoint**: `GET /health`

**Response**: `200 OK`
```json
{
  "status": "healthy",
  "rabbitMqConnected": true,
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**Example**:
```bash
curl http://localhost:8080/health
```

---

### 3. Create Order (RESTful)
Creates a new order and sends it to the message queue.

**Endpoint**: `POST /api/orders`

**Request Body**:
```json
{
  "OrderId": "ORD-001",
  "ProductId": "PROD-123",
  "Quantity": 5
}
```

**Validation Rules**:
- `OrderId`: Required, non-empty string
- `ProductId`: Required, non-empty string
- `Quantity`: Must be between 1 and 1000

**Response**: `202 Accepted`
```json
{
  "success": true,
  "message": "Order accepted and queued for processing",
  "data": {
    "orderId": "ORD-001",
    "productId": "PROD-123",
    "quantity": 5
  }
}
```

**Response**: `400 Bad Request` (Validation Error)
```json
{
  "success": false,
  "message": "Quantity must be greater than 0"
}
```

**Examples**:
```bash
# Valid order
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "OrderId": "ORD-001",
    "ProductId": "PROD-123",
    "Quantity": 5
  }'

# Invalid order (quantity = 0)
curl -X POST http://localhost:8080/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "OrderId": "ORD-002",
    "ProductId": "PROD-456",
    "Quantity": 0
  }'
```

---

### 4. Create Order (Legacy)
Alternative endpoint for backward compatibility.

**Endpoint**: `POST /order`

**Request Body**:
```json
{
  "OrderId": "ORD-001",
  "ProductId": "PROD-123",
  "Quantity": 5
}
```

**Response**: `202 Accepted`
```json
{
  "OrderId": "ORD-001",
  "ProductId": "PROD-123",
  "Quantity": 5
}
```

**Example**:
```bash
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{
    "OrderId": "ORD-001",
    "ProductId": "PROD-123",
    "Quantity": 5
  }'
```

---

### 5. Get Order Statistics
Get mock statistics about order processing (demonstration endpoint).

**Endpoint**: `GET /api/orders/stats`

**Response**: `200 OK`
```json
{
  "ordersProcessed": 542,
  "lastHour": 37,
  "averageQuantity": 6,
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**Example**:
```bash
curl http://localhost:8080/api/orders/stats
```

---

## 📊 Analytics API Endpoints

### 1. Health Check
Check if the Analytics service is running and connected to MongoDB.

**Endpoint**: `GET /health`

**Response**: `200 OK`
```json
{
  "status": "healthy",
  "mongodb": "connected",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

**Example**:
```bash
curl http://localhost:3000/health
```

---

### 2. Get All Orders
Retrieve a paginated list of all orders.

**Endpoint**: `GET /api/orders`

**Query Parameters**:
- `page` (optional): Page number (default: 1)
- `limit` (optional): Items per page (default: 10)

**Response**: `200 OK`
```json
{
  "success": true,
  "data": [
    {
      "_id": "65a5f8c9e1234567890abcde",
      "orderId": "ORD-001",
      "productId": "PROD-123",
      "quantity": 5,
      "createdAt": "2024-01-15T10:30:00.000Z",
      "processedAt": "2024-01-15T10:30:01.000Z",
      "status": "processed"
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 25,
    "totalPages": 3
  }
}
```

**Examples**:
```bash
# Get first page (10 items)
curl http://localhost:3000/api/orders

# Get second page with 20 items
curl http://localhost:3000/api/orders?page=2&limit=20

# Get all orders (max 100)
curl http://localhost:3000/api/orders?limit=100
```

---

### 3. Get Order by ID
Retrieve a specific order by its OrderId.

**Endpoint**: `GET /api/orders/:orderId`

**Response**: `200 OK`
```json
{
  "success": true,
  "data": {
    "_id": "65a5f8c9e1234567890abcde",
    "orderId": "ORD-001",
    "productId": "PROD-123",
    "quantity": 5,
    "createdAt": "2024-01-15T10:30:00.000Z",
    "processedAt": "2024-01-15T10:30:01.000Z",
    "status": "processed"
  }
}
```

**Response**: `404 Not Found`
```json
{
  "success": false,
  "error": "سفارش پیدا نشد"
}
```

**Example**:
```bash
curl http://localhost:3000/api/orders/ORD-001
```

---

### 4. Get Statistics
Get overall statistics about orders.

**Endpoint**: `GET /api/stats`

**Response**: `200 OK`
```json
{
  "success": true,
  "data": {
    "totalOrders": 25,
    "totalQuantity": 150,
    "recentOrders": [
      {
        "_id": "65a5f8c9e1234567890abcde",
        "orderId": "ORD-025",
        "productId": "PROD-789",
        "quantity": 3,
        "createdAt": "2024-01-15T10:35:00.000Z",
        "processedAt": "2024-01-15T10:35:01.000Z",
        "status": "processed"
      }
    ]
  }
}
```

**Example**:
```bash
curl http://localhost:3000/api/stats
```

---

### 5. Delete Order
Delete a specific order by its OrderId.

**Endpoint**: `DELETE /api/orders/:orderId`

**Response**: `200 OK`
```json
{
  "success": true,
  "message": "سفارش با موفقیت حذف شد"
}
```

**Response**: `404 Not Found`
```json
{
  "success": false,
  "error": "سفارش پیدا نشد"
}
```

**Example**:
```bash
curl -X DELETE http://localhost:3000/api/orders/ORD-001
```

---

## 🧪 Complete Testing Workflow

### Step 1: Create Multiple Orders
```bash
# Order 1
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-001", "ProductId": "PROD-123", "Quantity": 5}'

# Order 2
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-002", "ProductId": "PROD-456", "Quantity": 3}'

# Order 3
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{"OrderId": "ORD-003", "ProductId": "PROD-789", "Quantity": 10}'
```

### Step 2: View All Orders
```bash
curl http://localhost:3000/api/orders
```

### Step 3: View Statistics
```bash
curl http://localhost:3000/api/stats
```

### Step 4: Get Specific Order
```bash
curl http://localhost:3000/api/orders/ORD-001
```

### Step 5: Delete an Order
```bash
curl -X DELETE http://localhost:3000/api/orders/ORD-001
```

---

## 🔍 MongoDB Direct Access

You can also access MongoDB directly:

```bash
# Connect to MongoDB container
docker exec -it mongodb mongosh

# Use the analytics database
use analytics

# View all orders
db.orders.find().pretty()

# Count total orders
db.orders.countDocuments()

# Find orders with quantity > 5
db.orders.find({ quantity: { $gt: 5 } }).pretty()
```

---

## 📈 Performance Tips

### Indexes
The following indexes are automatically created:
- `orderId` (ascending) - for fast lookups by order ID
- `createdAt` (descending) - for efficient sorting by date

### Pagination
Always use pagination for large datasets:
```bash
# Get 50 orders per page
curl http://localhost:3000/api/orders?limit=50&page=1
```

---

## 🐛 Error Handling

All endpoints return consistent error responses:

**Format**:
```json
{
  "success": false,
  "error": "Error message here"
}
```

**Common Status Codes**:
- `200` - Success
- `404` - Resource not found
- `500` - Server error
