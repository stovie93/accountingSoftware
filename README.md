# Conservice Accounting Software

A multi-tenant accounting application for managing properties, bills, providers, utilities, and financial items.

## Technologies

### Backend
- **.NET 9.0** - Runtime and SDK
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 9** - ORM for database access
- **PostgreSQL 16** - Relational database
- **Npgsql** - PostgreSQL driver for .NET
- **JWT Bearer Authentication** - Token-based authentication
- **BCrypt.Net** - Password hashing
- **Swashbuckle** - Swagger/OpenAPI documentation
- **ClosedXML** - Excel (.xlsx) report export

### Frontend
- **React 19** - UI library
- **TypeScript 5.9** - Type-safe JavaScript
- **Vite 7** - Build tool and dev server
- **React Router DOM 7** - Client-side routing
- **TanStack React Query 5** - Server state management
- **Axios** - HTTP client
- **ESLint 9** - Code linting

### Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration

## Project Structure

```
accountingSoftware/
├── docker-compose.yml          # PostgreSQL database container
├── src/
│   ├── ConserviceAccounting.sln
│   ├── ConserviceAccounting.Api/        # Web API project
│   ├── ConserviceAccounting.Core/       # Domain models and interfaces
│   ├── ConserviceAccounting.Infrastructure/  # Data access and EF Core
│   └── web/                             # React frontend
└── tests/                               # Test projects
```

## Prerequisites

Before running the application, ensure you have the following installed:

1. **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop/)
2. **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
3. **Node.js 20+** - [Download](https://nodejs.org/)

## Getting Started

### Step 1: Start the Database

From the root directory, start the PostgreSQL container:

```bash
docker-compose up -d
```

This will start PostgreSQL on port 5432 with:
- Username: `postgres`
- Password: `postgres`
- Database: `conservice_accounting`

### Step 2: Run the Backend API

Navigate to the API project and run:

```bash
cd src/ConserviceAccounting.Api
dotnet run
```

The API will:
- Apply database migrations automatically
- Seed demo data (including demo tenant and admin user)
- Start on `http://localhost:5141` (HTTPS profile: `https://localhost:7013`)

Swagger documentation is available at: `http://localhost:5141/swagger`

### Step 3: Run the Frontend

In a new terminal, navigate to the web project:

```bash
cd src/web
npm install
npm run dev
```

The frontend will start on `http://localhost:5173`

### Step 4: Login

Open your browser to `http://localhost:5173` and login with:

- **Email:** `admin@demo.com`
- **Password:** `admin123`

## Available Scripts

### Backend

```bash
# Run the API
cd src/ConserviceAccounting.Api
dotnet run

# Run in watch mode (auto-restart on changes)
dotnet watch run

# Build the solution
cd src
dotnet build ConserviceAccounting.sln
```

### Frontend

```bash
cd src/web

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run linting
npm run lint
```

### Database

```bash
# Start PostgreSQL
docker-compose up -d

# Stop PostgreSQL
docker-compose down

# Stop and remove data volume
docker-compose down -v

# View logs
docker-compose logs -f postgres
```

## Configuration

### API Configuration

The API configuration is in `src/ConserviceAccounting.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=conservice_accounting;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ConserviceAccounting",
    "Audience": "ConserviceAccountingClients",
    "ExpirationMinutes": 60
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:5173", "http://localhost:3000"]
  }
}
```

### Frontend Configuration

The frontend talks to the API at the URL in the `VITE_API_URL` environment variable, falling back to `http://localhost:5141/api` (see `src/web/src/services/api.ts`).

To override it, create a `src/web/.env` file (this file is git-ignored):

```bash
VITE_API_URL=http://localhost:5141/api
```

## Features

- **Multi-tenant Architecture** - Isolated data per tenant
- **Entity Management** - Properties, Providers, Utilities, Bills
- **Items Tracking** - Financial transactions linked to entities
- **Dashboard Search** - Search across all entities and view linked items
- **Expandable Data Grids** - View entity relationships inline
- **Detail Panels** - Click any row to see full details
- **JWT Authentication** - Secure API access

## Troubleshooting

### Database connection issues

Ensure PostgreSQL is running:
```bash
docker-compose ps
```

Check if the database is accepting connections:
```bash
docker-compose logs postgres
```

### Port conflicts

If port 5432 is in use, modify `docker-compose.yml`:
```yaml
ports:
  - "5433:5432"  # Use 5433 instead
```

Then update the connection string in `appsettings.json`.

### CORS errors

Ensure the frontend URL is in the `Cors.AllowedOrigins` array in `appsettings.json`.

### Login not working

The seeder runs on every API startup and ensures the admin user exists with the correct password. If issues persist, try:
```bash
docker-compose down -v
docker-compose up -d
cd src/ConserviceAccounting.Api
dotnet run
```
