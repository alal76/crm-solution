FROM mcr.microsoft.com/playwright:v1.40.0-jammy

# Set working directory
WORKDIR /app/e2e-tests

# Copy package files
COPY package.json ./

# Install dependencies
RUN npm install

# Copy test files
COPY . .

# Install Playwright browsers
RUN npx playwright install --with-deps

# Set environment variables
ENV CI=true
ENV BASE_URL=http://192.168.0.9

# Create reports directory
RUN mkdir -p /app/e2e-tests/test-results /app/e2e-tests/playwright-report

# Default command - run all tests
CMD ["npm", "test"]
