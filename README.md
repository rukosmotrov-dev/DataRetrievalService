# DataRetrievalService

ASP.NET Core 9.0 Web API implementing a **multi-layered data retrieval service** with caching (Redis), file storage (with cleanup), database (MSSQL Server).\
Supports **JWT authentication**, role-based authorization (Admin/User),  and Docker-based deployment

## Why .NET 9 (STS) for this assignment?

> In short: I chose **.NET 9 (STS)** because it’s the latest stable release (as of Aug 2025) and enables the latest C# and ASP.NET Core features. For a small, short-lived assignment, the 18-month STS window is sufficient, and there’s a straightforward path to upgrade to the next LTS (**.NET 10**) if the project evolves.

- **Latest platform & language features:** The project uses .NET 9 with C# 13 to benefit from up-to-date capabilities (e.g., improved `lock`, `params` collections, quality-of-life language updates) and recent ASP.NET Core improvements (built-in OpenAPI generation, better perf/monitoring, broader Native AOT support).
- **Lower dependency surface:** ASP.NET Core 9 provides **built-in OpenAPI** (`Microsoft.AspNetCore.OpenApi`), reducing reliance on extra packages and improving trim/AOT friendliness.
- **Easy upgrade path:** If this evolves into something long-lived, the codebase can be upgraded to the next LTS (.NET 10) with a routine framework/package bump and test run.

---

## **Features**

- Layered data retrieval:
  1. **Cache** (Redis, 10 min TTL)
  2. **File Storage** (JSON, 30 min TTL) 
- When run API in VS the files are stored in DataRetrievalService\DataRetrievalService.Api\StorageFiles
- When run API in docker container the files are stored in app/StorageFiles
- Cleanup for the file storage run every 31 min base on the CleanupIntervalMinutes (configured in appsettings)
  3. **Database** (MSSQL in Docker)
- JWT authentication with Admin/User roles.
- Admin can insert/update; User can only retrieve.
- Repository Pattern, Factory Pattern, Dependency Injection, Decorator Pattern, Service Layer Pattern, Clean / Onion Architecture
- Swagger API documentation.
- Unit tests with `dotnet test`.
- Docker Compose for MSSQL, Redis, and API.
- FluentValidation
- Automapper
- Polly
- Postman (instruction attached in the end of this file)

---

## **Prerequisites**

- [Docker](https://docs.docker.com/get-docker/)
- [Docker Compose](https://docs.docker.com/compose/install/)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0) (if running API locally)
- [Git](https://git-scm.com/)

---

## **Environment Variables**

Copy `.env.example` to `.env` and adjust if needed.

| Variable              | Description                     | Default (dev)                                   |
| --------------------- | ------------------------------- | ----------------------------------------------- |
| `MSSQL_SA_PASSWORD`   | SA password for MSSQL container | `Local-Drs!2025#A9`                             |
| `JWT_KEY`             | Secret key for JWT signing      | `SuperLongRandomJwtKey_GeneratedOnceForDevOnly` |
| `SEED_ADMIN_PASSWORD` | Initial admin user password     | `Admin123!`                                     |
| `SEED_USER_PASSWORD`  | Initial standard user password  | `User123!`                                      |

---

### Running Locally with User Secrets (optional)

```bash
dotnet user-secrets init
dotnet user-secrets set "SeedUsers:AdminPassword" "Admin123!"
dotnet user-secrets set "SeedUsers:UserPassword"  "User123!"
dotnet user-secrets set "Jwt:Key" "SuperLongRandomJwtKey_GeneratedOnceForDevOnly"


## **Getting Started**

### 1️⃣ Clone the repository

```bash
git clone https://github.com/rukosmotrov-dev/DataRetrievalService.git
cd DataRetrievalService
```

### 2️⃣ Setup `.env`

```bash
cp .env.example .env
# edit if needed
```

---

## **Run with Docker Compose (API + MSSQL + Redis)**

```bash
docker compose up --build
```

- API: [http://localhost:8080](http://localhost:8080)
- Swagger: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- MSSQL: `localhost,1433` (user: `sa`, pass: from `.env`)
- Redis: `localhost:6379`

---

## **Run API in Visual Studio (with Docker DB & Redis)**

1. Start MSSQL and Redis only:
   ```bash
   docker compose up mssql redis
   ```
2. Open `DataRetrievalService.sln` in Visual Studio.
3. Set `DataRetrievalService.Api` as the startup project.
4. Run with `Development` profile.

---

## **Authentication**

Default seeded accounts (from `.env`):

- **Admin:**
  - Email: `admin@example.com`
  - Password: `Admin123!`
- **User:**
  - Email: `user@example.com`
  - Password: `User123!`

---

## **API Usage**

- Swagger: [http://localhost:8080/swagger](http://localhost:8080/swagger)
- Postman:
  1. Import `DataRetrievalService.postman_collection.json`.
  2. Import one of the Postman environment files (`DataRetrievalService_Docker.postman_environment.json` or `DataRetrievalService_Local_VS.postman_environment.json`).
  3. Authenticate via `/auth/login` to get JWT token.
  4. Call `/data/{id}` or `/data` endpoints.

---

## **Running Tests**

```bash
dotnet test
```

---

## **Project Structure**

```
DataRetrievalService.Api           # Web API project
DataRetrievalService.Application   # Application services
DataRetrievalService.Domain        # Entities and domain logic
DataRetrievalService.Infrastructure# Database, caching, file storage
DataRetrievalService.Tests         # Unit tests
docker-compose.yml                 # Multi-service config
```

---

## Postman
Collection and environments live in `docs/postman/`.

1. Import `docs/postman/DataRetrievalService.postman_collection.json`.
2. Import one environment:
   - Local VS: `docs/postman/environments/DataRetrievalService_Local_VS.postman_environment.json`
   - Docker:   `docs/postman/environments/DataRetrievalService_Docker.postman_environment.json`
3. Select the environment in Postman (top-right).
4. Run **Auth → Login (Admin)** to populate `{{token}}`.
5. Run **Data → Create** → saves `{{id}}`, then **Get/Update**.

---

## **Troubleshooting**

- **Port conflicts**: Change `ports` in `docker-compose.yml`.
- **MSSQL connection fails**: Ensure container is healthy (`docker ps`), check `.env` password.
- **Redis connection fails**: Ensure Redis container is running and port `6379` is free.
- **JWT errors**: Verify `JWT_KEY` in `.env` matches API config.