# EventFlow

EventFlow is a multi-tenant event-management API. Organizers manage events and ticket types within their tenant, while attendees can discover public events, place orders, and pay for tickets.

The repository is an actively developed project. It currently contains the backend API; there is no frontend application in this solution.

## Current Features

- Attendee and organizer registration, email confirmation, login, JWT access tokens, refresh-token rotation, logout, and password reset flows.
- Tenant-aware organizer event management: create, update, publish, and cancel events.
- Public event discovery with search, date filters, pagination, and public event details.
- Event ticket types with capacity, price, and sales-window management.
- Ticket orders, attendee order history, order cancellation, and organizer event-order views.
- Payment session initiation and payment-result handling through Paymob, including webhook HMAC verification.
- FluentValidation request validation, domain-level business rules, and consistent API problem responses.

## Solution Structure

```text
src/
  EventFlow.Api/             ASP.NET Core controllers, HTTP pipeline, and OpenAPI
  EventFlow.Application/     MediatR use cases, validation, DTOs, and interfaces
  EventFlow.Domain/          Domain entities, rules, errors, and events
  EventFlow.Infrastructure/  EF Core, SQL Server, authentication, email, caching, Paymob
```

The API uses controllers and MediatR. Application handlers depend on abstractions defined by the application layer; infrastructure supplies their implementations. Entity Framework Core persists data to SQL Server. The current solution file is `EventFlow.slnx`.

## Requirements

- .NET 10 SDK
- SQL Server or SQL Server LocalDB (the checked-in development connection string targets LocalDB)
- Paymob test credentials to exercise payment checkout and callbacks
- A reachable HTTPS callback URL, such as an ngrok tunnel, to receive Paymob webhooks locally

## Run Locally

1. Restore and build the solution:

	```powershell
	dotnet restore EventFlow.slnx
	dotnet build EventFlow.slnx
	```

2. Configure local secrets as described below.

3. Apply the EF Core migrations (install the `dotnet-ef` tool if it is not already available):

	```powershell
	dotnet ef database update --project src/EventFlow.Infrastructure --startup-project src/EventFlow.Api --context AppDbContext
	```

4. Start the API:

	```powershell
	dotnet run --project src/EventFlow.Api
	```

The HTTP launch profile listens at `http://localhost:5068`. In Development, the OpenAPI document is available at `/openapi/v1.json` and the Scalar API reference at `/scalar`.

## Configuration

The API reads standard ASP.NET Core configuration. For local development, keep credentials out of committed files and use User Secrets from the repository root:

```powershell
dotnet user-secrets set "JwtSettings:SigningKey" "<a-long-random-development-key>" --project src/EventFlow.Api
dotnet user-secrets set "Paymob:SecretKey" "<Paymob-secret-key>" --project src/EventFlow.Api
dotnet user-secrets set "Paymob:PublicKey" "<Paymob-public-key>" --project src/EventFlow.Api
dotnet user-secrets set "Paymob:HmacSecret" "<Paymob-HMAC-secret>" --project src/EventFlow.Api
```

The remaining configuration keys are:

| Key | Purpose |
| --- | --- |
| `ConnectionStrings:Default` | SQL Server connection string |
| `JwtSettings:Issuer` | JWT issuer |
| `JwtSettings:Audience` | JWT audience |
| `JwtSettings:DurationInMinutes` | Access-token lifetime |
| `Paymob:CardIntegrationIds` | Paymob card integration IDs |
| `Paymob:BaseUrl` | Paymob API base URL |
| `Paymob:RedirectionUrl` | Browser return URL configured in Paymob checkout |
| `Paymob:NotificationUrl` | Public URL for `/api/payments/webhook` |

Set the redirect and notification URLs to addresses that Paymob can reach. `EmailSettings` is used for SMTP outside Development. In Development, email delivery is replaced by a console logger, so registration and password-reset codes are written to application logs rather than sent.

Do not use checked-in configuration values as production secrets. Supply production credentials through the deployment environment's secret-management system and use a secure, unique JWT signing key.

## API Areas

| Area | Route prefix | Examples |
| --- | --- | --- |
| Authentication | `/api/auth` | Register, confirm email, login, refresh, logout, password recovery |
| Organizer events | `/api/events` | Create and manage tenant events; publish and cancel |
| Public discovery | `/api/public/events` | Browse and view public events |
| Ticket types | `/api/events/{eventId}/ticket-types` | Create and manage ticket options |
| Orders | `/api/orders` | Place, view, and cancel orders |
| Organizer orders | `/api/events/{eventId}/orders` | View an event's orders |
| Payments | `/api/payments` | Initiate checkout, receive webhooks, check redirect result |

Most attendee and organizer operations require a bearer access token. Payment webhooks and public event discovery are anonymous; webhook authenticity is checked by the payment integration.

## Development Notes

- The application and domain projects currently have no dedicated test project in the solution.
- Email, payment, and database behavior depends on the local configuration above; external payment callbacks require a public callback URL.
- API endpoint details and request/response schemas are available through the Development OpenAPI document and Scalar reference.
