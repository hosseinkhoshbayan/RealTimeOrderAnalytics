FROM node:20-alpine

WORKDIR /app

# کپی وابستگی‌ها
COPY package*.json ./
RUN npm install --production

# کپی کد
COPY . .

CMD ["npm", "start"]