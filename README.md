

# Portfolio Tracker Web API

A modern .NET 8 Web API featuring JWT authentication, role-based authorization, ASP.NET Identity, Entity Framework Core, Docker Compose, layered architecture, global exception handling, rate limiting, Serilog logging, AutoMapper, FluentValidation, and environment-driven configuration.

This project is designed as a clean, production-ready portfolio backend.

⸻

Table of Contents

* 	Features
* 	Architecture
* 	Requirements
* 	Local Development Setup
* 	Docker Setup
* 	Environment Variables
*   Database Migrations
* 	API Endpoints
* 	Health Checks
* 	Logging
* 	Security
* 	Password Hashing + Pepper
* 	License

⸻

## Features
* 	.NET 8 Web API
* 	JWT Authentication (access token + refresh token)
* 	Role-Based Authorization (Admin, User)
* 	ASP.NET Identity for user management
* 	EF Core + SQL Server
* 	Repository + Service + Unit of Work architecture
* 	Global Exception Middleware returning RFC7807 application/problem+json
* 	Rate Limiting (Fixed Window)
* 	Serilog structured logging with enriched request context
* 	AutoMapper
* 	FluentValidation with automatic model validation
* 	Docker Compose orchestration (API + SQL)
* 	User Secrets for secure local configuration

⸻

## Architecture
```python
WebApi/
├── src
│   └── Api
│       ├── Controllers
│       ├── Service
│       ├── Repo
│       ├── Interfaces
│       ├── Models
│       ├── Data
│       │   ├── AppDbContext.cs
│       │   └── DesignTimeFactory.cs
│       ├── Middleware
│       ├── Validators
│       ├── Mappers
│       ├── Options
│       └── Program.cs
├── tests
│   ├── Api.Tests
│   └── Api.IntegrationTests
├── docker-compose.yml
├── Dockerfile
├── .env
├── .env.example
├── .gitignore
├── NuGet.config
├── WebApi.sln
└── README.md
```

⸻

## Requirements
* 	.NET 8 SDK
* 	Docker + Docker Compose
* 	macOS, Linux, or Windows
* 	SQL Server (Dockerized)

⸻

## Local Development Setup

### 1. Start the SQL Server container

`docker compose up -d db
`

Accessible at:

`localhost:1433
`

⸻

### 2. Configure User Secrets

Navigate to the API project:

```
 cd src/Api
dotnet user-secrets init
```


Set required secrets:
```

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=Portfolio;User Id=sa;Password=<YourPassword>;TrustServerCertificate=True"

dotnet user-secrets set "JWT:SigningKey" "<64+ character signing key>"
dotnet user-secrets set "JWT:Issuer" "http://localhost"
dotnet user-secrets set "JWT:Audience" "http://localhost"

dotnet user-secrets set "Security:Pepper" "<your pepper string>"
```
Check:

`
dotnet user-secrets list
`

⸻

### 3. Apply database migrations

From the repository root:

`
dotnet ef database update --project src/Api --startup-project src/Api
`

⸻

### 4. Run the API
`
cd src/Api
`

`
dotnet run
`

Swagger UI:

`
http://localhost:5000/swagger
`

⸻

## Docker Setup (API + SQL)

### Run everything fully containerized:
`
docker-compose up -d
`

API available at:

`
http://localhost:8080
`

⸻

## Environment Variables

### .env (used only by Docker)
```python

SA_PASSWORD=YourStrongPassword
JWT_SIGNING_KEY=YOUR_64_CHAR_KEY
ALLOWED_ORIGINS=http://localhost:5173;http://localhost:3000
SECURITY_PEPPER=YOUR_PEPPER_VALUE
```
Local development does not use .env.

Local uses **User Secrets**.

⸻

## Database Migrations

### Add a new migration
`
dotnet ef migrations add <Name> --project src/Api --startup-project src/Api
`

### Apply migration
`
dotnet ef database update --project src/Api --startup-project src/Api
`

⸻

## API Endpoints (Summary)

### Account
```
POST /api/Account/register   
POST /api/Account/login      
POST /api/Account/refresh     
POST /api/Account/revoke 
```
### Stocks
```
GET /api/Stock
POST /api/Stock
PUT /api/Stock/{id}
DELETE /api/Stock/{id}
```
### Portfolio
```
GET /api/Portfolio
POST /api/Portfolio
```
### Comments
```
GET /api/Comment
POST /api/Comment
DELETE /api/Comment/{id}
```
Full documentation available via Swagger.

⸻

## Health Checks
```
/health/live
/health/ready
```

⸻

## Logging

The project uses Serilog with:
* 	RequestId
* 	UserId
* 	ClientIp
* 	QueryString (sanitized)
* 	Exception stack traces (dev only)
* 	Structured logs suitable for Seq / Elastic / Datadog

Global middleware enriches every request with correlation IDs.

⸻

## Security
* 	64+ character JWT signing key
* 	ASP.NET Identity password hashing
* 	Custom peppered hasher
* 	Role-based authorization
* 	CORS restrictions
* 	HTTPS redirection in production
* 	Database credentials never included in source control

⸻

## Password Hashing with Pepper

The API uses ASP.NET Identity’s PBKDF2 password hashing combined with an additional secret pepper.
* 	The pepper is not stored in the database.
* 	It is only provided through environment variables.
*   This makes precomputed attacks (rainbow tables) ineffective even if hashes are leaked.

### Pepper sources:
* 	User Secrets (local development)
* 	.env (Docker) for production-like environments

The final stored password =
Hash(Password + Pepper + IdentitySalt)

⸻

## License

This project is provided for portfolio and educational purposes.


