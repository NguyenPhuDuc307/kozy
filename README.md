# Backend API Project

A full-stack application with .NET 9 backend API and React frontend client.

## Project Structure

```txt
kozy-api/           # Backend API (.NET 9)
kozy-api.Tests/     # Backend unit tests
kozy-client/        # Frontend client (React + TypeScript)
```

## Quick Start

### Prerequisites

- .NET 9 SDK
- Node.js 18+ and npm
- PostgreSQL or SQL Server (optional for development)

### Backend Development (.NET API)

```bash
# Navigate to backend
cd kozy-api

# Restore packages
dotnet restore

# Update database (if using Entity Framework)
dotnet ef database update

# Run API
dotnet run
```

The API will be available at `https://localhost:7230` or `http://localhost:5230`

### Frontend Development (React Client)

```bash
# Navigate to frontend
cd kozy-client

# Install dependencies
npm install

# Copy environment file
cp .env.example .env

# Start development server
npm run dev
```

The client will be available at `http://localhost:5173`

## Backend (.NET API)

### Configuration

Update `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  },
  "JwtSettings": {
    "Key": "your-jwt-secret-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience",
    "ExpiryHours": 24
  }
}
```

### Running Tests

```bash
cd kozy-api.Tests
dotnet test
```

### Database Migrations

```bash
# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Frontend (React Client)

### Environment Variables

Create `.env` file in `kozy-client/` directory:

```bash
# API Configuration
VITE_API_BASE_URL=http://localhost:5230/api

# Environment
NODE_ENV=development
```

### Production Build

```bash
cd kozy-client

# Build for production
npm run build

# Preview production build
npm run preview
```

## Docker

### Backend API

```bash
cd kozy-api
docker build -t kozy-api .
docker run -p 5230:8080 kozy-api
```

### Frontend Client

```bash
cd kozy-client

# Build Docker image
npm run docker:build

# Run container
npm run docker:run

# Or use docker-compose
npm run docker:compose
```

### Full Stack with Docker Compose

```bash
# Build and run all services
docker-compose up --build -d

# Stop all services
docker-compose down

# View logs
docker-compose logs -f
```

## Testing

### Backend Tests

```bash
cd kozy-api.Tests
dotnet test --collect:"XPlat Code Coverage"
```

### Generate Coverage Report

```bash
# Run the coverage script
./generate-coverage-report.sh
```
