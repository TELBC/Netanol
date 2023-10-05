# Stage 1: Nuxt JS container
FROM node:18 AS builder
# Set the working directory inside the container to /app
WORKDIR /app

COPY package*.json ./
RUN npm ci
# Copy the entire project directory to the container
COPY . .
RUN npm run build

# Stage 2 - start
# Create a new stage for the final container
FROM node:18 AS final
WORKDIR /app
# Copy the package.json from the builder stage to the final stage
COPY --from=builder /app/package*.json ./
# Copy the built application files from the builder stage to the final stage
COPY --from=builder /app/.output ./.output
CMD ["node", ".output/server/index.mjs"]