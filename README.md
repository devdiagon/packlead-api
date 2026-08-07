# Packlead API

[![CI](https://github.com/devdiagon/packlead-api/actions/workflows/ci.yml/badge.svg)](https://github.com/devdiagon/packlead-api/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/download)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-EF%20Core-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Firebase Auth](https://img.shields.io/badge/Auth-Firebase-FFCA28?logo=firebase&logoColor=white)](https://firebase.google.com/)

A backend service built with **ASP.NET Core (.NET 10)** for managing the CRUD operations of orders and deliveries for the [Packlead](https://github.com/devdiagon/packlead) mobile app, with support for two roles: **Admin** and **Dispatcher**.

---

## Tech Stack

- **ASP.NET Core** (.NET 10) — REST API with controllers
- **PostgreSQL** + **Entity Framework Core** — persistence
- **Firebase Admin SDK** — token-based authentication and authorization
- **FluentValidation** — request validation
- **Scalar** — interactive API documentation (OpenAPI)
- **xUnit**, **Moq**, **Testcontainers** — unit and integration tests

---

## Architecture

The project follows a **layered monolith** approach, split into four projects:

```
Packlead.Domain/            # Pure entities and business rules, no external dependencies
Packlead.Application/       # Use cases (commands/queries), DTOs, interfaces
Packlead.Infrastructure/    # EF Core, repositories, Firebase integration
Packlead.Api/                # Controllers, middleware, HTTP entry point
```

The `Domain` and `Application` layers never depend on `Infrastructure` or `Api`, which keeps the business logic testable in isolation.

**Key domain concepts:**

- Orders (`Order`) follow a linear state flow: `Pending → Shipped → Delivered`, encapsulated in domain methods instead of free field assignment.
- Dispatchers (`Dispatcher`) are identified with the system's own UUID, decoupled from the Firebase UID.
- The `Admin` role has no table of its own: it lives only as a *custom claim* in Firebase.
- API errors follow a consistent envelope:

```json
{
  "status": 404,
  "error": "NotFound",
  "message": "Order with id 'abc123' was not found."
}
```

---

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for local PostgreSQL and for integration tests with Testcontainers)
- A [Firebase](https://firebase.google.com/) project with Authentication enabled

---

## Local setup

1. Clone the repository:

   ```bash
   git clone https://github.com/devdiagon/packlead-api.git
   cd Packlead
   ```

2. Start PostgreSQL with Docker Compose:

   ```bash
   docker compose up -d
   ```

3. Configure local credentials (connection string, Firebase credentials) via [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables.

4. Apply the database migrations:

   ```bash
   dotnet ef database update --project Packlead.Infrastructure --startup-project Packlead.Api
   ```

5. Run the API:

   ```bash
   dotnet run --project Packlead.Api
   ```

6. In the development environment, interactive API documentation is available at `/scalar`.

---

## Tests

The project has three levels of automated tests:

| Project | What it covers |
|---|---|
| `Packlead.Domain.Tests` | Pure business rules (state transitions, invariants) |
| `Packlead.Application.Tests` | Use cases with mocked dependencies |
| `Packlead.Api.IntegrationTests` | Real endpoints against a PostgreSQL database spun up with Testcontainers |

To run the full suite:

```bash
dotnet test
```

Integration tests require Docker running locally.

---
