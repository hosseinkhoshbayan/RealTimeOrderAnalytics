#!/bin/bash

# Script to test the entire microservices system

echo "🧪 ===== Testing Real-Time Order Analytics Platform ====="
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Test counter
TESTS_PASSED=0
TESTS_FAILED=0

# Function to test endpoint
test_endpoint() {
    local name=$1
    local url=$2
    local method=${3:-GET}
    local data=$4
    
    echo -n "Testing: $name... "
    
    if [ "$method" = "POST" ]; then
        response=$(curl -s -w "\n%{http_code}" -X POST "$url" \
            -H "Content-Type: application/json" \
            -d "$data")
    else
        response=$(curl -s -w "\n%{http_code}" "$url")
    fi
    
    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✓ PASSED${NC} (HTTP $http_code)"
        ((TESTS_PASSED++))
        return 0
    else
        echo -e "${RED}✗ FAILED${NC} (HTTP $http_code)"
        ((TESTS_FAILED++))
        return 1
    fi
}

echo "Step 1: Health Checks"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
test_endpoint "Order API Health" "http://localhost:8080/"
test_endpoint "Analytics API Health" "http://localhost:3000/health"
echo ""

echo "Step 2: Create Orders"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
test_endpoint "Create Order 1" "http://localhost:8080/order" "POST" \
    '{"OrderId": "ORD-TEST-001", "ProductId": "PROD-123", "Quantity": 5}'

test_endpoint "Create Order 2" "http://localhost:8080/order" "POST" \
    '{"OrderId": "ORD-TEST-002", "ProductId": "PROD-456", "Quantity": 3}'

test_endpoint "Create Order 3" "http://localhost:8080/order" "POST" \
    '{"OrderId": "ORD-TEST-003", "ProductId": "PROD-789", "Quantity": 10}'

echo ""
echo -e "${YELLOW}⏳ Waiting 3 seconds for messages to be processed...${NC}"
sleep 3
echo ""

echo "Step 3: Retrieve Orders"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
test_endpoint "Get All Orders" "http://localhost:3000/api/orders"
test_endpoint "Get Order by ID" "http://localhost:3000/api/orders/ORD-TEST-001"
test_endpoint "Get Statistics" "http://localhost:3000/api/stats"
echo ""

echo "Step 4: Display Results"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Get and display orders
echo "📦 Recent Orders:"
curl -s http://localhost:3000/api/orders?limit=5 | python3 -m json.tool 2>/dev/null || \
    curl -s http://localhost:3000/api/orders?limit=5

echo ""
echo ""

# Get and display stats
echo "📊 Statistics:"
curl -s http://localhost:3000/api/stats | python3 -m json.tool 2>/dev/null || \
    curl -s http://localhost:3000/api/stats

echo ""
echo ""

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🏁 Test Summary"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo -e "Tests Passed: ${GREEN}$TESTS_PASSED${NC}"
echo -e "Tests Failed: ${RED}$TESTS_FAILED${NC}"
echo ""

if [ $TESTS_FAILED -eq 0 ]; then
    echo -e "${GREEN}🎉 All tests passed!${NC}"
    exit 0
else
    echo -e "${RED}❌ Some tests failed!${NC}"
    exit 1
fi