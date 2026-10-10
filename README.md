# ClinicManagement

ClinicManagement is a clinic-management REST API built with .NET 9 and SQL Server, accompanied by a React and TypeScript web frontend. The repository demonstrates a simplified Clean Architecture, ASP.NET Core Identity and JWT authentication, role-based API authorization, EF Core persistence, and automated service-level tests.

> **Project status:** This is a portfolio and development project, not a production-ready clinical system. The web application is an early UI foundation: login is connected to the API, while clinic data pages remain placeholders and CRUD workflows are not implemented in the frontend.

## Contents

- [Solution architecture](#solution-architecture)
- [Technology stack](#technology-stack)
- [Implemented capabilities](#implemented-capabilities)
- [API reference](#api-reference)
- [Authentication and authorization](#authentication-and-authorization)
- [Appointment rules and concurrency](#appointment-rules-and-concurrency)
- [Prerequisites and configuration](#prerequisites-and-configuration)
- [Database migrations](#database-migrations)
- [Run the API](#run-the-api)
- [Run the frontend](#run-the-frontend)
- [Build and test](#build-and-test)
- [Known limitations](#known-limitations)

## Solution architecture

The solution contains five .NET projects and a separate frontend:

| Project | Responsibility |
| --- | --- |
| `ClinicManagement.Domain` | Clinic entities, the shared base entity, and domain enums. |
| `ClinicManagement.Application` | DTOs, service contracts and implementations, persistence abstractions, and application business rules. |
| `ClinicManagement.Infrastructure` | EF Core and SQL Server persistence, repositories, Unit of Work, Identity storage, JWT creation, and role seeding. |
| `ClinicManagement.API` | ASP.NET Core host, controllers, authentication and authorization, CORS, Swagger, and exception middleware. |
| `ClinicManagement.Tests` | xUnit and Moq tests for application services. |
| `ClinicManagement.Web` | Separate React, TypeScript, and Vite frontend. |

The project references follow this dependency direction:

```text
ClinicManagement.API          -> Application, Infrastructure
ClinicManagement.Infrastructure -> Application, Domain
ClinicManagement.Application  -> Domain
ClinicManagement.Domain       -> no project references
ClinicManagement.Tests        -> Application
```

Application currently contains service implementations as well as service contracts. It depends on persistence abstractions rather than Infrastructure.

## Technology stack

All .NET projects target `net9.0`.

| Area | Technologies |
| --- | --- |
| Backend | ASP.NET Core Web API, .NET 9, C# |
| Architecture | Simplified Clean Architecture, dependency-injected application services |
| Persistence | EF Core 9, SQL Server, ASP.NET Core Identity |
| Authentication | JWT bearer authentication and role-based authorization |
| API documentation | Swagger/OpenAPI via Swashbuckle |
| Backend tests | xUnit, Moq, Microsoft.NET.Test.Sdk |
| Frontend | React, TypeScript, Vite, React Router, Tailwind CSS v4 |
| Frontend support libraries | Axios, TanStack Query, React Hook Form, Zod, Lucide React |

## Implemented capabilities

- Authentication endpoints for account registration and login.
- JWT access token issuance with identity and assigned-role claims.
- Controller-level authorization for `Admin`, `Doctor`, and `Receptionist`.
- CRUD API endpoints for doctors, patients, appointments, medical records, and prescriptions.
- Patient search and paginated listing.
- Appointment business rules and SQL Server uniqueness protection for scheduled appointments.
- Global exception handling and development-only Swagger UI.
- A responsive frontend shell with routing and a Login page connected to the API. The Login session and access token are held in memory and are lost on page reload.

The frontend's Register, dashboard, patients, doctors, appointments, medical records, and prescriptions screens are currently placeholders. No frontend CRUD operations or dashboard statistics are implemented.

## API reference

The API route prefix is `/api`. Protected endpoints require a valid bearer token and one of the roles shown below; the roles listed on a controller are alternatives.

| Module | Endpoints | Authentication / role |
| --- | --- | --- |
| Authentication | `POST /api/Auth/register`, `POST /api/Auth/login` | Public |
| Doctors | `GET /api/Doctors`, `GET /api/Doctors/{id}`, `POST /api/Doctors`, `PUT /api/Doctors/{id}`, `DELETE /api/Doctors/{id}` | `Admin` or `Doctor` |
| Patients | `GET /api/Patients`, `GET /api/Patients/{id}`, `POST /api/Patients`, `PUT /api/Patients/{id}`, `DELETE /api/Patients/{id}` | `Admin`, `Doctor`, or `Receptionist` |
| Appointments | `GET /api/Appointments`, `GET /api/Appointments/{id}`, `POST /api/Appointments`, `PUT /api/Appointments/{id}`, `DELETE /api/Appointments/{id}` | `Admin`, `Doctor`, or `Receptionist` |
| Medical records | `GET /api/MedicalRecords`, `GET /api/MedicalRecords/{id}`, `POST /api/MedicalRecords`, `PUT /api/MedicalRecords/{id}`, `DELETE /api/MedicalRecords/{id}` | `Admin` or `Doctor` |
| Prescriptions | `GET /api/Prescriptions`, `GET /api/Prescriptions/{id}`, `POST /api/Prescriptions`, `PUT /api/Prescriptions/{id}`, `DELETE /api/Prescriptions/{id}` | `Admin` or `Doctor` |

Create requests return `201 Created`; successful reads and updates return `200 OK`; successful deletes return `204 No Content`. A missing resource returns `404 Not Found`. Invalid model input and business-rule failures return `400 Bad Request`; invalid login credentials return `401 Unauthorized`; requests without valid authentication return `401`, and authenticated users without an allowed role receive `403`. Unexpected server errors return a generic `500` response.

`POST /api/Auth/login` is rate-limited to five requests per client IP in a one-minute sliding window; excess requests receive `429 Too Many Requests`.

### Patient pagination and search

`GET /api/Patients` accepts:

| Query parameter | Default | Constraints |
| --- | --- | --- |
| `pageNumber` | `1` | At least `1` |
| `pageSize` | `10` | Between `1` and `100` |
| `search` | None | Optional partial match on patient name |

Example:

```http
GET /api/Patients?pageNumber=1&pageSize=10&search=Moh
```

The response is a paged object containing `items`, `pageNumber`, `pageSize`, `totalCount`, and `totalPages`. Filtering, counting, ordering, and pagination are composed in the EF Core query.

Swagger/OpenAPI is enabled only in the Development environment. Its UI is available at `/swagger` on the selected API origin.

## Authentication and authorization

`POST /api/Auth/login` accepts `email` and `password`. A successful response contains `accessToken`, `email`, and `expiresAtUtc`. The API signs the JWT with the configured key and includes user identity, email, token ID, and the account's assigned roles as claims. The token is sent to protected endpoints in the `Authorization: Bearer <token>` header.

`POST /api/Auth/register` accepts an email and password and returns the same response shape on success. Registration does **not** assign a role. A newly registered user cannot access role-protected clinic endpoints until an allowed role is assigned.

The web Login page stores the access token only in application memory; it is not written to `localStorage` or `sessionStorage`. Reloading the page ends the frontend session. The frontend also uses a client-side route guard, but API authentication and authorization remain the security boundary.

The configured roles are `Admin`, `Doctor`, and `Receptionist`. A startup hosted service ensures those roles exist and assigns the `Doctor` role to an **already existing** Identity account specified by `RoleSeeding:DoctorUserEmail`. It does not create that account. On a fresh Identity database, the configured account must be bootstrapped before API startup; otherwise the hosted service fails startup. The repository does not include a separate account-bootstrap utility.

## Appointment rules and concurrency

- New appointments are created with status `Scheduled`.
- Doctor and patient records must exist, and a new or rescheduled appointment date must be in the future.
- Conflict checks apply to `Scheduled` appointments only, using exact equality of `AppointmentDate` for the same doctor or patient.
- Updating only the status to `Completed`, `Cancelled`, or `NoShow` does not apply the future-date rule; this allows lifecycle updates to past appointments.
- Changing scheduling data validates the doctor, patient, and future date. Conflict checks run when the appointment's final status is `Scheduled`.
- The update checks exclude the appointment being updated.
- SQL Server filtered unique indexes provide database-level protection for scheduled doctor/date and patient/date pairs. The Unit of Work maps violations of those named indexes to business-rule errors; unrelated database update errors are not mapped to those appointment conflicts.

The conflict checks and unique indexes protect exact appointment timestamps; they do not model appointment duration or overlapping time ranges.

## Prerequisites and configuration

- .NET 9 SDK.
- SQL Server accessible to the API.
- Node.js and npm compatible with the Vite version specified by the frontend package manifest.
- EF Core command-line tools (`dotnet-ef`) for applying migrations.

The API reads its SQL Server connection from `ConnectionStrings:DefaultConnection`. The checked-in default targets a local SQL Server instance named `localhost`, database `ClinicManagementDb`, using Windows trusted authentication. Override this setting for a different SQL Server or authentication method.

The API requires the `JwtSettings` values `Key`, `Issuer`, `Audience`, and `ExpirationInMinutes`. `Key` must contain at least 32 UTF-8 bytes. The Development configuration already supplies non-secret issuer, audience, and expiration settings; provide the signing key through User Secrets, not tracked files:

```powershell
dotnet user-secrets set "JwtSettings:Key" "<private-random-key-at-least-32-UTF8-bytes>" `
  --project .\ClinicManagement.API\ClinicManagement.API.csproj
```

The API project already has a `UserSecretsId`. Configure the existing account used by role seeding through a private configuration source if it differs from the checked-in development setting:

```powershell
dotnet user-secrets set "RoleSeeding:DoctorUserEmail" "<email-of-an-existing-identity-account>" `
  --project .\ClinicManagement.API\ClinicManagement.API.csproj
```

Do not commit signing keys, passwords, real connection-string credentials, or private account details. The API validates JWT configuration at startup.

## Database migrations

The repository contains these EF Core migrations:

1. `20261005192346_InitialCreate` — clinic tables and relationships.
2. `20261006211946_AddIdentity` — ASP.NET Core Identity schema.
3. `20261008122637_AddAppointmentConcurrencyIndexes` — filtered unique indexes for scheduled appointment conflicts.

If `dotnet ef` is not installed, install a version compatible with EF Core 9:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.0
```

After confirming the target SQL Server connection and backing up any important data, initialize or update the database from the repository root:

```powershell
dotnet ef database update `
  --project .\ClinicManagement.Infrastructure\ClinicManagement.Infrastructure.csproj `
  --startup-project .\ClinicManagement.API\ClinicManagement.API.csproj
```

This command changes the target database. Do not run it against a database whose state or connection has not been confirmed.

## Run the API

From the repository root:

```powershell
dotnet restore .\ClinicManagement.sln
dotnet run --project .\ClinicManagement.API\ClinicManagement.API.csproj --launch-profile https
```

The `https` launch profile binds HTTPS at `https://localhost:7224` and HTTP at `http://localhost:5099`. Swagger is available at:

```text
https://localhost:7224/swagger
```

The HTTP-only launch profile uses `http://localhost:5099`. HTTPS redirection is enabled by the API pipeline.

## Run the frontend

The frontend is a separate project under `ClinicManagement.Web`. From the repository root:

```powershell
Set-Location .\ClinicManagement.Web
npm.cmd ci
npm.cmd run dev
```

Vite's default development URL is `http://localhost:5173` (it may select another port if that port is occupied). The API URL is configured by `VITE_API_BASE_URL`; `.env.example` points to `http://localhost:5099`, which is also the Axios fallback. To use a local override, create `.env.local` from `.env.example` and set the API origin there. Do not place secrets or tokens in Vite environment files; values with the `VITE_` prefix are exposed to the browser bundle.

The API CORS policy currently allows the origin `http://localhost:5173`, the `POST` method, and the `Content-Type` and `Authorization` headers. This is sufficient for the current login request. If Vite uses a different origin, or frontend API calls need methods such as `GET`, `PUT`, or `DELETE`, update the API CORS policy accordingly.

The frontend's Login and protected route shell are implemented. Registration UI, data-fetching screens, CRUD workflows, logout controls, and dashboard statistics are not connected to backend features yet.

## Build and test

Build the .NET solution:

```powershell
dotnet build .\ClinicManagement.sln
```

Run the test project:

```powershell
dotnet test .\ClinicManagement.sln
```

The test project currently contains two test source files:

- `AppointmentServiceTests.cs` covers appointment creation, missing doctor/patient references, past dates, scheduled doctor/patient conflicts, non-scheduled conflict behavior, valid status values, lifecycle updates, rescheduling, conflict checks, and missing appointments. It uses mocked repositories and Unit of Work.
- `MedicalRecordAndPrescriptionServiceTests.cs` covers missing doctor or patient references during medical-record and prescription creation and updates. It also uses mocks.

These are service-level unit tests, not SQL Server integration tests; they do not prove database concurrency behavior. The frontend production build can be run separately:

```powershell
Set-Location .\ClinicManagement.Web
npm.cmd run build
```

## Known limitations

- The frontend data-management pages are placeholders; API CRUD endpoints exist, but the frontend does not yet implement those workflows.
- The frontend session is memory-only and ends on reload.
- A new registered Identity user receives no role automatically.
- Initial startup requires the role seeder's configured Doctor account to exist in the Identity database.
- CORS currently permits only `POST` for the Vite development origin; additional API methods require an explicit policy change.
- Appointment tests use mocks; there is no SQL Server-backed concurrency test in the test project.
- The application has not been represented as production-ready and has no claim of clinical, regulatory, or operational certification.
