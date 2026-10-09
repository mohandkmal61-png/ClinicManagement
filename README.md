# ClinicManagement

ClinicManagement is a .NET 9 REST API for managing core clinic records and workflows. It provides endpoints for doctors, patients, appointments, medical records, and prescriptions, together with Identity-based authentication, JWT bearer tokens, role-based authorization, and SQL Server persistence.

## Features

- **Authentication:** ASP.NET Core Identity registration and login endpoints.
- **Login throttling:** A per-client-IP sliding-window limit applies to `POST /api/Auth/login` (five attempts per minute); excess requests receive HTTP 429. The current policy does not partition by account and should be revisited behind a reverse proxy or when configuring trusted forwarded headers.
- **JWT:** Signed bearer tokens include user identity and assigned role claims.
- **Role-based authorization:** Controller-level access rules for `Admin`, `Doctor`, and `Receptionist`.
- **Clinic records:** CRUD endpoints for doctors, patients, appointments, medical records, and prescriptions.
- **Appointment rules:** A doctor and patient must exist; new appointments must be in the future; a scheduled appointment cannot conflict with the same doctor's or patient's appointment at the same date and time. Completed, cancelled, and no-show appointments do not block a new booking. Update checks exclude the appointment being updated.
- **Patient pagination and search:** The patient collection endpoint supports database-side pagination and partial name search.
- **Global exception handling:** Maps missing records to 404, business-rule failures to 400, and unexpected exceptions to a generic 500 response.
- **Logging:** Uses the built-in `ILogger` system. Unexpected exceptions include server-side exception details and request metadata in logs; appointment creation logs operational identifiers without logging patient data.
- **Swagger/OpenAPI:** Interactive API documentation is enabled in Development and has JWT Bearer authorization support.
- **Unit tests:** xUnit tests cover AppointmentService business rules using mocked repositories and Unit of Work. Tests do not use SQL Server.

## Architecture

The solution is split into five projects:

| Project | Responsibility |
| --- | --- |
| `ClinicManagement.Domain` | Clinic entities, shared domain base type, and domain enums. |
| `ClinicManagement.Application` | DTOs, service contracts and implementations, persistence abstractions, and application-level business rules. |
| `ClinicManagement.Infrastructure` | EF Core and SQL Server persistence, repository and Unit of Work implementations, Identity storage, JWT token creation, and role seeding. |
| `ClinicManagement.API` | ASP.NET Core host, controllers, authentication/authorization pipeline, Swagger, and global exception middleware. |
| `ClinicManagement.Tests` | Unit tests for the Application layer using xUnit and Moq; references Application and does not connect to the database. |

Project dependency direction:

```text
ClinicManagement.API -> ClinicManagement.Application
ClinicManagement.API -> ClinicManagement.Infrastructure
ClinicManagement.Infrastructure -> ClinicManagement.Application
ClinicManagement.Infrastructure -> ClinicManagement.Domain
ClinicManagement.Application -> ClinicManagement.Domain
ClinicManagement.Domain -> no project
ClinicManagement.Tests -> ClinicManagement.Application
```

The API controllers depend on Application service interfaces and DTOs. Infrastructure provides persistence and Identity implementations. Application currently contains both service contracts and service implementations; its code uses abstractions rather than depending on Infrastructure.

## Technology stack

All projects target `net9.0`.

| Project | Direct NuGet packages |
| --- | --- |
| Domain | None |
| Application | `Microsoft.Extensions.Logging.Abstractions` 9.0.0 |
| Infrastructure | `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 9.0.0; `Microsoft.EntityFrameworkCore` 9.0.0; `Microsoft.EntityFrameworkCore.SqlServer` 9.0.0; `Microsoft.EntityFrameworkCore.Tools` 9.0.0; `Microsoft.Extensions.Hosting.Abstractions` 9.0.0; `Microsoft.Extensions.Options.ConfigurationExtensions` 9.0.0; `System.IdentityModel.Tokens.Jwt` 7.1.2 |
| API | `Microsoft.AspNetCore.Authentication.JwtBearer` 9.0.0; `Microsoft.EntityFrameworkCore.Design` 9.0.0; `Swashbuckle.AspNetCore` 6.6.2 |
| Tests | `Microsoft.NET.Test.Sdk` 17.12.0; `Moq` 4.20.72; `xunit` 2.9.2; `xunit.runner.visualstudio` 2.8.2 |

ASP.NET Core and the built-in logging abstractions are used without an external logging provider.

## Roles and authorization

The configured roles are:

- `Admin`
- `Doctor`
- `Receptionist`

Controller-level authorization currently permits:

| Controller | Allowed roles |
| --- | --- |
| `DoctorsController` | Admin, Doctor |
| `PatientsController` | Admin, Doctor, Receptionist |
| `AppointmentsController` | Admin, Doctor, Receptionist |
| `MedicalRecordsController` | Admin, Doctor |
| `PrescriptionsController` | Admin, Doctor |
| `AuthController` | Public registration and login endpoints |

Multiple roles in one authorization attribute are alternatives: a user needs any one of the listed roles. Infrastructure creates a configured role if it is missing and assigns the `Doctor` role to the existing account selected by `RoleSeeding:DoctorUserEmail`. The seeder does not create that account; it reports a startup error if the configured account is absent.

## API endpoints

All routes use the `api` prefix. Except for authentication, the endpoints below require a bearer token and one of the roles listed above.

| Resource | Endpoints |
| --- | --- |
| Authentication | `POST /api/Auth/register`, `POST /api/Auth/login` |
| Doctors | `GET /api/Doctors`, `GET /api/Doctors/{id}`, `POST /api/Doctors`, `PUT /api/Doctors/{id}`, `DELETE /api/Doctors/{id}` |
| Patients | `GET /api/Patients`, `GET /api/Patients/{id}`, `POST /api/Patients`, `PUT /api/Patients/{id}`, `DELETE /api/Patients/{id}` |
| Appointments | `GET /api/Appointments`, `GET /api/Appointments/{id}`, `POST /api/Appointments`, `PUT /api/Appointments/{id}`, `DELETE /api/Appointments/{id}` |
| Medical records | `GET /api/MedicalRecords`, `GET /api/MedicalRecords/{id}`, `POST /api/MedicalRecords`, `PUT /api/MedicalRecords/{id}`, `DELETE /api/MedicalRecords/{id}` |
| Prescriptions | `GET /api/Prescriptions`, `GET /api/Prescriptions/{id}`, `POST /api/Prescriptions`, `PUT /api/Prescriptions/{id}`, `DELETE /api/Prescriptions/{id}` |

Successful reads and updates return 200, creates return 201, and deletes return 204. Missing resources return 404. Invalid model input returns 400. Login with invalid credentials returns 401.

### Patient pagination and search

`GET /api/Patients` accepts:

- `pageNumber` (default `1`, minimum `1`)
- `pageSize` (default `10`, allowed range `1`-`100`)
- `search` (optional partial match on patient name)

Example:

```http
GET /api/Patients?pageNumber=1&pageSize=10&search=Moh
```

The response contains `items`, `pageNumber`, `pageSize`, `totalCount`, and `totalPages`. Filtering, counting, ordering, and pagination are executed by EF Core in the database; the full patient set is not loaded before pagination.

## Authentication flow

1. Register through `POST /api/Auth/register` with an email and password, or sign in through `POST /api/Auth/login`.
2. Successful registration and login responses include an access token, email, and UTC expiration time. The password and password hash are never returned.
3. Send the token on protected requests using the HTTP header:

   ```http
   Authorization: Bearer <access-token>
   ```

4. Swagger UI provides the **Authorize** control for bearer tokens.

Registration does not assign a role. A newly registered account therefore cannot access role-protected clinic controllers until a role is assigned through the existing Identity setup.

The JWT configuration section is `JwtSettings` and contains `Key`, `Issuer`, `Audience`, and `ExpirationInMinutes`. In Development, supply `JwtSettings:Key` through User Secrets; the API project is configured with a `UserSecretsId`, and ASP.NET Core loads its User Secrets only in Development. From the repository root, set a private random key of at least 32 UTF-8 bytes with:

```powershell
dotnet user-secrets set "JwtSettings:Key" "<your-private-random-key-at-least-32-bytes>" --project .\ClinicManagement.API\ClinicManagement.API.csproj
```

Do not put real secrets in tracked configuration or commit them. For other environments, supply the key through that environment's secret configuration mechanism.

## Database and configuration

The API uses EF Core 9 with SQL Server and a shared `ClinicDbContext` for clinic entities and ASP.NET Core Identity. The configured connection string key is `ConnectionStrings:DefaultConnection`; the checked-in local default points to SQL Server at `localhost` and uses Windows trusted authentication.

Before running the API:

1. Install the .NET 9 SDK and make a compatible SQL Server instance available.
2. Set `ConnectionStrings:DefaultConnection` for that environment.
3. Set a real development JWT key through the API project's User Secrets as shown above, and configure the corresponding issuer, audience, and expiration.
4. Set `RoleSeeding:DoctorUserEmail` to an account that already exists in the Identity database. The seeder creates the configured roles when needed and assigns `Doctor` to this existing account.
5. Apply the existing migrations if the target database has not yet been initialized.

Existing migrations are `InitialCreate` and `AddIdentity`. To apply them from the repository root with the EF Core command-line tool installed:

```powershell
dotnet ef database update `
  --project .\ClinicManagement.Infrastructure\ClinicManagement.Infrastructure.csproj `
  --startup-project .\ClinicManagement.API\ClinicManagement.API.csproj
```

Do not apply migrations blindly to a database with an unknown state. Back up important data and confirm the target connection before updating a database.

## Run the API

From the repository root:

```powershell
dotnet restore .\ClinicManagement.sln
dotnet run --project .\ClinicManagement.API\ClinicManagement.API.csproj --launch-profile https
```

The HTTPS launch profile uses `https://localhost:7224` and opens Swagger at:

```text
https://localhost:7224/swagger
```

Swagger is enabled only in the Development environment.

## Build and test

Build the complete solution:

```powershell
dotnet build .\ClinicManagement.sln
```

Run all tests:

```powershell
dotnet test .\ClinicManagement.sln
```

The current test project contains 11 AppointmentService unit tests. They use mocks and do not require a running API or a SQL Server connection.
