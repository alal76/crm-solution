# SARCH-036: crm-script-runner Docker image
# Multi-stage build: compile TypeScript, then run minimal production image.

FROM node:20-alpine AS builder
WORKDIR /app

# Install dependencies first for better layer caching
COPY package.json tsconfig.json ./
RUN npm ci

# Copy source and compile
COPY src/ ./src/
RUN npm run build

# ── Production stage ──────────────────────────────────────────────────────────
FROM node:20-alpine
WORKDIR /app

# Copy compiled output and production node_modules
COPY --from=builder /app/dist ./dist
COPY --from=builder /app/node_modules ./node_modules
COPY package.json ./

EXPOSE 4000

# Run as non-root for security
USER node

CMD ["node", "dist/index.js"]
