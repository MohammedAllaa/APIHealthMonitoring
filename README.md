# API Health Monitoring System

An Enterprise-grade **API Health Monitoring & Analytics Platform** built with **ASP.NET Core (.NET 8)** following **Clean Architecture** principles. The system provides automated real-time API probing, proactive status calculation, configurable threshold alerts, interactive operational metrics, analytical performance reporting, and role-based access control (RBAC).

---

## 📋 Table of Contents
- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture & Layering](#architecture--layering)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Database Schema & Data Model](#database-schema--data-model)
- [API Endpoints Reference](#api-endpoints-reference)
- [Security & Authentication (RBAC)](#security--authentication-rbac)
- [Background Monitoring Engine](#background-monitoring-engine)
- [Getting Started & Local Setup](#getting-started--local-setup)
- [Default Seed Credentials](#default-seed-credentials)

---

## 🚀 Overview

The **API Health Monitoring System** enables DevOps, System Reliability Engineers (SREs), and API managers to continuously monitor external and internal API endpoints across multiple environments (`Production`, `UAT`, `QA`, `Development`).

It regularly probes registered HTTP/HTTPS endpoints, tracks status codes and response latency, calculates availability percentages, automatically triggers alerts upon performance degradation or downtime, and generates detailed analytical reports.

---

## ✨ Key Features

1. **Authentication & Identity Management (JWT + Refresh Tokens)**
   - Secure user authentication with JSON Web Tokens (JWT).
   - In-memory/database refresh token rotation and revocation.
   - User account lifecycle management (Activation/Deactivation by Administrators).
   - Password change and user management endpoints.

2. **Role-Based Access Control (RBAC)**
   - **Administrator**: Full administrative control — register/update/delete endpoints, modify threshold configs, resolve alerts, manage user accounts.
   - **Viewer**: Read-only access — view operational dashboards, monitoring logs, alerts, and historical performance reports.

3. **API Endpoint Registry & Configuration**
   - Register endpoints with metadata (`Name`, `BaseUrl`, `HealthEndpoint`, `HttpMethod`, `ExpectedStatusCode`, `TimeoutSeconds`, `IntervalSeconds`, `Environment`, `ServiceOwner`).
   - Per-endpoint custom monitoring rules (`SlowThresholdMs`, `CriticalThresholdMs`, `FailureCountLimit`, `AvailabilityThreshold`).

4. **Automated Background Health Engine**
   - ASP.NET Core `BackgroundHostedService` executing periodic non-blocking health checks.
   - Measures response time (ms), HTTP status codes, payload sizes, and connection errors.
   - Supports manual on-demand execution (`POST /api/endpoints/{id}/check-now`).

5. **Dynamic Health Status Evaluation**
   - Automatically transition endpoint status between:
     - 🟢 `Healthy`: Normal latency & 2xx responses.
     - 🟡 `Warning`: Response time exceeds slow threshold or single check failure.
     - 🔴 `Critical`: Response time exceeds critical threshold or consecutive failures hit `FailureCountLimit`.
     - ⚪ `Unknown`: Unchecked or initial state.

6. **Proactive Alert Engine**
   - Automated creation of `Warning` and `Critical` alerts based on check results.
   - Tracks resolution states (`Open` vs `Closed`).
   - Supports manual resolution by Administrators with notes.

7. **Real-time Operations Dashboard**
   - Total endpoints breakdown (Healthy, Warning, Critical, Inactive).
   - Overall availability percentage calculation.
   - Active open alerts summary.
   - API cards and endpoint performance trends.

8. **Historical Analytics & Reporting**
   - **Daily Health Report**: Date-filtered check breakdown, success rates, min/max/avg latency.
   - **Weekly Trend Report**: Aggregated availability and latency trends across days.
   - **Monthly Performance Report**: Top 5 best & worst performing APIs ranked by availability & response time.

9. **Mock Testing Environment (`FakeServices.API`)**
   - Includes a standalone mock API project to simulate real-world response latency, success responses, and failures for testing monitoring rules.

---

## 🏗️ Architecture & Layering

The solution follows **Clean Architecture** (Onion Architecture) with strict layer isolation:

```
                  ┌──────────────────────────────┐
                  │    APIHealthMonitoring.API   │  (Controllers, Middleware, Swagger, Program.cs)
                  └──────────────┬───────────────┘
                                 │
           ┌─────────────────────┴─────────────────────┐
           ▼                                           ▼
┌─────────────────────────────┐             ┌──────────────────────────────────┐
│ .Infrastructure             │             │ .Persistence                     │
│ (Background Engine, Probes, │             │ (EF Core DbContext, Migrations,  │
│  Alerts, JWT, Identity)     │             │  Unit of Work, Repositories)     │
└──────────┬──────────────────┘             └──────────────────┬───────────────┘
           │                                                   │
           └─────────────────────┬─────────────────────────────┘
                                 ▼
                  ┌──────────────────────────────┐
                  │ .Application                 │ (DTOs, Interfaces, Specifications, Logic)
                  └──────────────┬───────────────┘
                                 ▼
                  ┌──────────────────────────────┐
                  │ .Domain                      │ (Entities, Enums, Base Models)
                  └──────────────────────────────┘
```

- **Domain**: Pure C# domain entities (`ApiEndpoint`, `HealthCheck`, `Alert`, `MonitoringConfiguration`, `ApplicationUser`, `ApplicationRole`) and core Enums (`ApiHealthStatus`, `AlertSeverity`, `AlertStatus`, `Environment`, `HttpMethod`).
- **Application**: Contains DTOs, service interfaces, pagination abstractions (`PaginatedResult<T>`), specifications, and business contracts.
- **Infrastructure**: Implements external concerns — HTTP health probes (`HealthCheckExecutor`), hosted service (`MonitoringBackgroundService`), JWT token generation, alert evaluation algorithms, reporting engines.
- **Persistence**: Database access layer using EF Core, `AppDbContext`, repository patterns, Unit of Work, data seeding (`DatabaseSeeder`).
- **API**: ASP.NET Core Web API layer containing controllers, middleware configuration, Swagger UI setup, and dependency injection wiring.

---

## 🛠️ Tech Stack

- **Framework**: .NET 8 / .NET 9 C#
- **ORM**: Entity Framework Core 8.x
- **Database**: Microsoft SQL Server
- **Identity & Security**: ASP.NET Core Identity + JWT Bearer Tokens
- **Documentation**: Swagger / OpenAPI with Bearer Auth support
- **Background Execution**: `IHostedService` (`BackgroundService`)
- **HTTP Client**: `IHttpClientFactory`

---

## 📂 Project Structure

```
APIHealthMonitoring/
├── APIHealthMonitoring.sln
├── SRC/
│   ├── APIHealthMonitoring.API/                # Entry point, Web API controllers
│   │   ├── Controllers/                        # Auth, Endpoints, HealthChecks, Alerts, Dashboard, Reports, Users, Config
│   │   ├── Program.cs                          # Middleware & Service DI registration
│   │   └── appsettings.json                    # Connection strings & JWT configs
│   ├── APIHealthMonitoring.Application/        # Contracts & DTOs
│   │   ├── DTOs/                               # Auth, Endpoints, Alerts, HealthChecks, Dashboard, Reports
│   │   ├── Interfaces/                         # Service & Repository interfaces
│   │   └── Specifications/                     # Pagination & filter specifications
│   ├── APIHealthMonitoring.Domain/             # Domain entities & business enums
│   │   ├── Entities/                           # ApiEndpoint, HealthCheck, Alert, MonitoringConfiguration, User, Role
│   │   └── Enums/                              # ApiHealthStatus, AlertSeverity, Environment, etc.
│   ├── APIHealthMonitoring.Infrastructure/     # Background monitoring, HTTP execution, Identity, Reporting
│   │   ├── HealthChecks/                       # MonitoringBackgroundService, HealthCheckExecutor, Evaluator
│   │   ├── Alerts/                             # AlertEvaluator, AlertService
│   │   └── Identity/                           # JwtTokenGenerator, AuthService, UserManagementService
│   └── APIHealthMonitoring.Persistence/        # EF Core DbContext, Repositories, Migrations, Seeder
│       ├── Data/                               # AppDbContext, DatabaseSeeder
│       └── Repositories/                       # Generic & Specialized Repositories
└── Fake/
    └── FakeServices.API/                       # Mock API endpoints for probing & demonstration
```

---

## 🗄️ Database Schema & Data Model

### Core Entities & Relationships

1. **`ApiEndpoint`**: Represents a monitored API route.
   - One-to-One with `MonitoringConfiguration`.
   - One-to-Many with `HealthCheck`.
   - One-to-Many with `Alert`.
2. **`MonitoringConfiguration`**: Custom monitoring parameters (slow/critical thresholds, consecutive failure limits, availability SLAs).
3. **`HealthCheck`**: Individual log entry recorded every time an HTTP probe executes (Timestamp, Status Code, Latency in ms, Success boolean, Response payload size, Error message).
4. **`Alert`**: System notification raised when an endpoint degrades or fails (`Severity`, `Message`, `Status`, `GeneratedAt`, `ResolvedAt`, `ResolutionNotes`).
5. **`ApplicationUser` & `ApplicationRole`**: Extended Identity entities supporting full name, active status, refresh token storage, and expiration tracking.

---

## 🔑 Security & Authentication (RBAC)

The application enforces **JWT Bearer Authentication**.

### Roles & Permissions Matrix

| Endpoint Group | Method / Route | Administrator | Viewer | Public |
| :--- | :--- | :---: | :---: | :---: |
| **Auth** | `POST /api/auth/login` | ✅ | ✅ | ✅ |
| **Auth** | `POST /api/auth/refresh-token` | ✅ | ✅ | ✅ |
| **Auth** | `POST /api/auth/register` | ✅ | ❌ | ❌ |
| **Auth** | `POST /api/auth/logout` | ✅ | ✅ | ❌ |
| **Auth** | `POST /api/auth/change-password` | ✅ | ✅ | ❌ |
| **Users** | `GET /api/users` | ✅ | ❌ | ❌ |
| **Users** | `PUT /api/users/{id}/activate` | ✅ | ❌ | ❌ |
| **Users** | `PUT /api/users/{id}/deactivate` | ✅ | ❌ | ❌ |
| **Endpoints** | `GET /api/endpoints` | ✅ | ✅ | ❌ |
| **Endpoints** | `POST /api/endpoints` | ✅ | ❌ | ❌ |
| **Endpoints** | `PUT /api/endpoints/{id}` | ✅ | ❌ | ❌ |
| **Endpoints** | `DELETE /api/endpoints/{id}` | ✅ | ❌ | ❌ |
| **Health Checks** | `GET /api/health-checks` | ✅ | ✅ | ❌ |
| **Health Checks** | `POST /api/endpoints/{id}/check-now` | ✅ | ❌ | ❌ |
| **Alerts** | `GET /api/alerts` | ✅ | ✅ | ❌ |
| **Alerts** | `PUT /api/alerts/{id}/resolve` | ✅ | ❌ | ❌ |
| **Dashboard** | `GET /api/dashboard/summary` | ✅ | ✅ | ❌ |
| **Reports** | `GET /api/reports/*` | ✅ | ✅ | ❌ |

---

## 📡 API Endpoints Reference

### 🔐 Auth & Identity (`/api/auth`)
- `POST /api/auth/login` — Authenticate and receive JWT + Refresh Token.
- `POST /api/auth/register` — Create a new user (Admin only).
- `POST /api/auth/refresh-token` — Request new JWT using refresh token.
- `POST /api/auth/logout` — Revoke active refresh token.
- `POST /api/auth/change-password` — Change authenticated user password.

### 👥 User Management (`/api/users`)
- `GET /api/users` — List all registered users.
- `GET /api/users/{id}` — Get single user details.
- `PUT /api/users/{id}/activate` — Activate user account.
- `PUT /api/users/{id}/deactivate` — Deactivate user account & revoke refresh token.

### 🌐 Endpoints Management (`/api/endpoints`)
- `GET /api/endpoints` — Paged & filterable list of monitored endpoints.
- `POST /api/endpoints` — Register new API endpoint.
- `GET /api/endpoints/{id}` — Endpoint details by ID.
- `PUT /api/endpoints/{id}` — Update endpoint configuration.
- `DELETE /api/endpoints/{id}` — Delete endpoint registration.
- `PUT /api/endpoints/{id}/activate` — Enable endpoint monitoring.
- `PUT /api/endpoints/{id}/deactivate` — Pause endpoint monitoring.

### ⚙️ Monitoring Configuration (`/api/endpoints/{id}/config`)
- `GET /api/endpoints/{id}/config` — Get threshold rules.
- `POST /api/endpoints/{id}/config` — Create threshold rules.
- `PUT /api/endpoints/{id}/config` — Update threshold rules.
- `DELETE /api/endpoints/{id}/config` — Reset rules to system defaults.

### 🩺 Health Checks (`/api/health-checks` & `/api/endpoints/{id}/...`)
- `GET /api/health-checks` — Global paged health check logs.
- `GET /api/health-checks/{id}` — Single check detail.
- `GET /api/endpoints/{id}/health-checks` — Check logs for specific endpoint.
- `GET /api/endpoints/{id}/status` — Current status summary for endpoint.
- `POST /api/endpoints/{id}/check-now` — Trigger immediate manual health check.

### 🚨 Alerts Management (`/api/alerts`)
- `GET /api/alerts` — Paged list of generated alerts (Filter by status/severity).
- `GET /api/alerts/{id}` — Alert detail.
- `GET /api/endpoints/{id}/alerts` — Alerts for specific API.
- `PUT /api/alerts/{id}/resolve` — Mark alert as resolved/closed with optional resolution notes.

### 📊 Dashboard & Metrics (`/api/dashboard`)
- `GET /api/dashboard/summary` — Global system health metrics (Total APIs, Healthy, Warning, Critical, SLA, Open Alerts).
- `GET /api/dashboard/apis` — Paginated API overview cards for UI.
- `GET /api/endpoints/{id}/stats` — Comprehensive performance statistics for single endpoint.

### 📈 Analytical Reports (`/api/reports`)
- `GET /api/reports/daily?date=yyyy-MM-dd` — Daily performance breakdown.
- `GET /api/reports/weekly?weekStart=yyyy-MM-dd` — Weekly uptime and latency trends.
- `GET /api/reports/monthly?year=2026&month=7` — Top 5 best & worst performing APIs.

---

## ⚙️ Background Monitoring Engine

The `MonitoringBackgroundService` operates in a background loop:
1. Retrieves all **active** endpoints (`IsActive == true`).
2. Checks if their specified `IntervalSeconds` has elapsed since `LastCheckedAt`.
3. Issues async HTTP request using `HttpClient` with timeout configured via `TimeoutSeconds`.
4. Records result into `HealthChecks` table.
5. `HealthStatusEvaluator` updates `CurrentStatus` (`Healthy`, `Warning`, `Critical`).
6. `AlertEvaluator` checks threshold conditions and generates new `Alert` entries when appropriate.

---

## ⚙️ Getting Started & Local Setup

### Prerequisites
- [.NET 8.0 SDK or .NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or Full instance)
- Visual Studio 2022 / VS Code / JetBrains Rider

### Configuration (`appsettings.json`)
Ensure your SQL Server connection string in `SRC/APIHealthMonitoring.API/appsettings.json` points to a valid server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=APIHealthMonitoringDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "APIHealthMonitoring",
    "Audience": "APIHealthMonitoringClients",
    "ExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Running the Database Migration & Seeder
The database will automatically run migrations and populate seed data (roles, users, endpoints, 30-day historical health check logs, sample alerts) on initial startup!

Alternatively, you can apply migrations manually via dotnet CLI:
```bash
dotnet ef database update --project SRC/APIHealthMonitoring.Persistence --startup-project SRC/APIHealthMonitoring.API
```

### Running the API Project
```bash
dotnet run --project SRC/APIHealthMonitoring.API
```
Navigate to `https://localhost:7196/swagger` (or port shown in terminal output) to open the interactive **Swagger UI**.

### Running the Fake Mock Services (Optional)
To test live health probing against mock HTTP endpoints:
```bash
dotnet run --project Fake/FakeServices.API
```

---

## 🔑 Default Seed Credentials

Upon initial database seeding, the following default accounts are created:

| Role | Email | Password | Access Rights |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin@healthmonitor.com` | `Admin@12345` | Full Control (Create, Update, Delete, Config, Users) |
| **Viewer** | `viewer@healthmonitor.com` | `Viewer@12345` | Read-Only (Dashboard, Reports, Logs, Alerts) |
| **Administrator** | `admin@apihealthmonitoring.com` | `Admin@12345` | Secondary Admin Account |

---

## 📄 License & Attribution

Developed for **Elswedy Internship Project** — Enterprise API Health Monitoring Platform.
