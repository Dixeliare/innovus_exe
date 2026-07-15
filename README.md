# Innovus_exe

A .NET 8 Web API solution for a piano learning center, covering class scheduling, attendance, student consultation requests, learning resources (sheet music library), room/instrument management, and reporting. The solution contains a Web_API project (ASP.NET Core), Services (business logic), Repository (data access), DTOs, and SQL scripts for DB initialization and seed data.

## Tech
- .NET 8 (Microsoft.NET.Sdk / ASP.NET Core)
- EF Core (Npgsql for PostgreSQL, also references for Sqlite/SqlServer)
- PostgreSQL (Docker Compose)
- JWT authentication
- Swagger (OpenAPI)
- Azure Blob Storage (optional for file storage)
- Rate limiting and hosted background services

## Projects (solution)
- Web_API: ASP.NET Core API, controllers, middleware, Program.cs
- Services: business logic and background services
- Repository: EF Core repositories & UnitOfWork
- DTOs: data transfer objects
- Other folders: SQL scripts for DB schema and seed data

## Important files
- Web_API/appsettings.json — configuration (contains connection strings/JWT keys; DO NOT commit production secrets)
- docker-compose.yml — runs API container + Postgres; mounts `init.sql` to initialize DB
- init.sql, postgresql_data.sql, fixing.sql, rollback_sheet_relationship.sql — DB initialization and seed scripts
- Web_API/Program.cs — DI, authentication, rate limiting, Swagger, hosts background services

## Prerequisites
- .NET 8 SDK
- Docker & Docker Compose (recommended for local DB)
- (Optional) PostgreSQL if running DB outside Docker

## Quick start (using Docker Compose)
1. From repo root run:
   docker-compose up --build
2. API will be available at: http://localhost:5000 (mapped to container port 8080)
3. Open Swagger UI: http://localhost:5000/swagger

The docker-compose mounts `init.sql` into Postgres `docker-entrypoint-initdb.d/` so the DB will be seeded on first startup.

## Run locally without Docker
1. Start PostgreSQL and create a database (or use an existing host)
2. Set environment variables (examples):
   - ConnectionStrings__DefaultConnection=Host=<host>;Port=<port>;Database=<db>;Username=<user>;Password=<pass>
   - Jwt__Key=<strong-secret>
   - Jwt__Issuer=http://localhost:5000
   - Jwt__Audience=http://localhost:5000
   - AzureBlobStorage__ConnectionString=<optional>
   - AzureBlobStorage__ContainerName=<optional>
   - ASPNETCORE_ENVIRONMENT=Development
3. Build and run API:
   dotnet build
   dotnet run --project Web_API
4. Browse Swagger: http://localhost:<port>/swagger (port configured via ASPNETCORE_URLS or launch settings)

Notes: Do NOT hardcode secrets. Use environment variables or a secure secret store.

## Authentication
- Login endpoint (no UI): POST /api/User/Login
- Provide username & password (see controllers) to receive a JWT token
- Use `Authorization: Bearer {token}` for protected endpoints
- Only endpoints with [Authorize] require tokens; others may be public

## Database & Migrations
- The project uses EF Core with Npgsql provider.
- If you prefer EF migrations:
  - Install dotnet-ef tools: dotnet tool install --global dotnet-ef
  - Create/Apply migrations: dotnet ef migrations add Initial --project Repository --startup-project Web_API
  - Update DB: dotnet ef database update --project Repository --startup-project Web_API

## Key behavior & features
- Rate limiting policies (global and stricter LoginPolicy) are configured in Program.cs
- Background services generate schedules and statistics (hosted services registered in Program.cs)
- Azure Blob Storage is supported via configuration section `AzureBlobStorage` and IFileStorageService implementation
- Swagger is enabled by config `EnableSwagger` or in Development

## API surface (controllers)
24 controllers under `Web_API/Controllers`, grouped by domain:
- **Scheduling & attendance:** Schedule, Week, Day, DayOfWeekLookup, Timeslot, OpeningSchedule, ClassSession, Attendance, AttendanceStatus
- **Classes & users:** Class, User, Role, Gender
- **Consultation:** ConsultationRequest, ConsultationTopic
- **Learning resources:** Sheet, SheetMusic, UserFavoriteSheet, Document, Genre, Instrument
- **Facilities:** Room
- **Reporting:** Statistic

Use Swagger to explore exact request/response shapes and required DTOs for each endpoint.

## Security & Secrets
- Remove secrets from appsettings.json before publishing, or commit a version that contains only placeholders.
- Generate a strong Jwt__Key (at least 32+ bytes) and store it securely.

## Development notes
- Developed solo with JetBrains Rider; includes a .sln file and multiple csproj projects (Web_API, Services, Repository, DTOs, Tests).
- If deploying to Azure, set `ASPNETCORE_ENVIRONMENT=Production` or `Azure` and provide Azure-specific environment variables (AZURE_DOMAIN, AzureBlobStorage settings). Program.cs reads `appsettings.Azure.json` optionally when in Production/Azure.

## Testing
- `Tests` project (MSTest + Moq) covers the schedule creation/update logic in `ScheduleService`: validation, duplicate-month-year conflict handling, and cascading week generation/deletion.
- Run with: `dotnet test` (from repo root or the `Tests` folder).

## Troubleshooting
- API can't connect to DB → verify `ConnectionStrings__DefaultConnection` and that PostgreSQL is reachable.
- Swagger not visible → ensure `EnableSwagger` is true, or run in Development.
- EF migration/initialization errors → check the application logs.

## License
No license specified.