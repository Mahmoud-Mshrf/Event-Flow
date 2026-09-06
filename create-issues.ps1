$issues = @(
    @{
        Title = "Day 1 - Solution Structure, Core Entities and Multi-Tenancy"
        Body = @'
## Goal

Establish the foundation of the system, including the solution architecture, core domain entities, EF Core, and tenant isolation infrastructure.

## Checklist

- [ ] Create .NET solution and project structure
- [ ] Establish Domain / Application / Infrastructure / API boundaries
- [ ] Create core domain entities
- [ ] Configure EF Core
- [ ] Create Tenant entity
- [ ] Add TenantId to tenant-scoped entities
- [ ] Configure EF Core global query filters
- [ ] Establish tenant resolution from authenticated user context
- [ ] Create initial database migration
- [ ] Verify tenant-scoped data access

## Definition of Done

- [ ] Solution builds successfully
- [ ] EF Core is configured
- [ ] Tenant model exists
- [ ] Tenant isolation infrastructure is established
- [ ] Initial migration succeeds

## Commit Target

`chore: initialize solution and multi-tenancy foundation`
'@
    },

    @{
        Title = "Day 2 - Identity, JWT and Roles"
        Body = @'
## Goal

Implement authentication and authorization for the three MVP roles.

## Checklist

- [ ] Configure ASP.NET Core Identity
- [ ] Implement registration and login
- [ ] Implement JWT authentication
- [ ] Configure role-based authorization
- [ ] Create Tenant Owner role
- [ ] Create Check-in Staff role
- [ ] Define attendee authentication/registration flow
- [ ] Associate tenant users with their tenant
- [ ] Add tenant information to authenticated identity/claims
- [ ] Protect management endpoints
- [ ] Verify unauthorized role access is rejected

## Definition of Done

- [ ] User can authenticate
- [ ] JWT is generated
- [ ] Roles are enforced server-side
- [ ] Tenant identity is available from the authenticated user

## Commit Target

`feat: implement identity jwt authentication and roles`
'@
    },

    @{
        Title = "Day 3 - Event Management and Lifecycle"
        Body = @'
## Goal

Implement event creation and the complete event state machine.

## Checklist

- [ ] Create Event entity
- [ ] Implement Event CRUD
- [ ] Implement Draft state
- [ ] Implement Published state
- [ ] Implement Registration Open state
- [ ] Implement Registration Closed state
- [ ] Implement Completed state
- [ ] Implement Cancelled state
- [ ] Implement state transition rules
- [ ] Validate publishing requirements
- [ ] Implement state-dependent edit rules
- [ ] Prevent incomplete events from being published
- [ ] Enforce tenant ownership of events

## Definition of Done

- [ ] Owner can create an event
- [ ] Event lifecycle works correctly
- [ ] Invalid transitions are rejected
- [ ] State-dependent editing rules work
- [ ] Events cannot cross tenant boundaries

## Commit Target

`feat: implement event lifecycle and management`
'@
    },

    @{
        Title = "Day 4 - Ticket Types and Sales Windows"
        Body = @'
## Goal

Allow organizers to define ticket types and control when tickets can be sold.

## Checklist

- [ ] Create TicketType entity
- [ ] Associate ticket types with events
- [ ] Implement ticket type CRUD
- [ ] Add price
- [ ] Add capacity
- [ ] Add sales start and end
- [ ] Validate ticket prices
- [ ] Validate capacity
- [ ] Validate sales windows
- [ ] Prevent invalid ticket configuration
- [ ] Enforce event and tenant ownership

## Definition of Done

- [ ] Multiple ticket types can exist per event
- [ ] Ticket types have independent capacity
- [ ] Sales windows are enforced
- [ ] Invalid configurations are rejected

## Commit Target

`feat: implement event ticket types and sales windows`
'@
    },

    @{
        Title = "Day 5 - Concurrency-Safe Ticket Capacity"
        Body = @'
## Goal

Prevent ticket overselling under concurrent purchase attempts.

## Checklist

- [ ] Choose concurrency strategy
- [ ] Implement optimistic concurrency or database-level protection
- [ ] Protect capacity decrement operation
- [ ] Ensure capacity cannot become negative
- [ ] Handle concurrent purchase conflicts
- [ ] Return appropriate failure when capacity is exhausted
- [ ] Write automated concurrency test
- [ ] Simulate multiple buyers attempting to purchase final tickets
- [ ] Verify sold tickets never exceed capacity

## Definition of Done

- [ ] Capacity is concurrency-safe
- [ ] Overselling is structurally prevented
- [ ] Concurrent test passes
- [ ] Last-ticket scenario is proven by automated testing

## Commit Target

`feat: make ticket capacity concurrency safe`
'@
    },

    @{
        Title = "Day 6 - Public Event Discovery"
        Body = @'
## Goal

Build public event discovery and event details.

## Checklist

- [ ] Implement public event listing
- [ ] Implement event details endpoint
- [ ] Show organizer and tenant name
- [ ] Show event information
- [ ] Show ticket types
- [ ] Show ticket prices
- [ ] Show remaining capacity
- [ ] Implement search
- [ ] Implement filtering
- [ ] Enforce public visibility rules
- [ ] Exclude Draft events
- [ ] Exclude Private events

## Definition of Done

- [ ] Visitors can browse events without authentication
- [ ] Search and filtering work
- [ ] Only eligible public events appear
- [ ] Event details expose required information

## Commit Target

`feat: implement public event discovery and details`
'@
    },

    @{
        Title = "Day 7 - Registration and Order Creation"
        Body = @'
## Goal

Implement attendee registration and server-controlled order creation.

## Checklist

- [ ] Implement attendee registration flow
- [ ] Create Order entity
- [ ] Create order items and selected tickets
- [ ] Generate unique order number
- [ ] Capture attendee information
- [ ] Associate order with event
- [ ] Associate order with tenant
- [ ] Calculate total server-side
- [ ] Never trust client-provided price
- [ ] Validate ticket availability
- [ ] Define order lifecycle and statuses

## Definition of Done

- [ ] Attendee can create an order
- [ ] Order total is calculated server-side
- [ ] Client cannot manipulate price
- [ ] Order belongs to correct tenant and event

## Commit Target

`feat: implement attendee registration and order creation`
'@
    },

    @{
        Title = "Day 8 - Payment Gateway Integration"
        Body = @'
## Goal

Integrate a real payment gateway sandbox/test environment.

## Checklist

- [ ] Select payment provider
- [ ] Configure sandbox/test credentials
- [ ] Implement payment gateway abstraction
- [ ] Create payment initiation flow
- [ ] Create payment request from an order
- [ ] Handle successful payment flow
- [ ] Handle failed payment flow
- [ ] Store provider transaction/reference ID
- [ ] Ensure client cannot directly mark an order as Paid
- [ ] Document payment flow

## Definition of Done

- [ ] Order can initiate real sandbox payment
- [ ] Gateway interaction works
- [ ] Payment status is not controlled by client
- [ ] Provider transaction information is persisted

## Commit Target

`feat: integrate payment gateway sandbox`
'@
    },

    @{
        Title = "Day 9 - Payment Webhooks and Idempotency"
        Body = @'
## Goal

Make payment confirmation reliable and idempotent.

## Checklist

- [ ] Implement payment webhook endpoint
- [ ] Verify webhook signature
- [ ] Reject invalid signatures
- [ ] Extract provider event or transaction ID
- [ ] Persist processed webhook/event IDs
- [ ] Detect duplicate webhook delivery
- [ ] Make processing idempotent
- [ ] Update order to Paid only after verified webhook
- [ ] Prevent duplicate ticket generation
- [ ] Prevent duplicate confirmation
- [ ] Write automated idempotency test
- [ ] Send identical webhook multiple times
- [ ] Verify only one successful processing occurs

## Definition of Done

- [ ] Webhook signature is verified
- [ ] Duplicate webhooks are safely ignored
- [ ] Automated idempotency test passes
- [ ] Client cannot fake payment confirmation

## Commit Target

`feat: implement verified idempotent payment webhooks`
'@
    },

    @{
        Title = "Day 10 - Digital Tickets and Signed QR Codes"
        Body = @'
## Goal

Generate secure digital tickets after successful payment.

## Checklist

- [ ] Create Ticket entity
- [ ] Generate ticket number
- [ ] Associate ticket with order
- [ ] Associate ticket with attendee
- [ ] Associate ticket with event
- [ ] Implement ticket status
- [ ] Generate QR code
- [ ] Design secure QR payload
- [ ] Sign and protect QR payload
- [ ] Implement QR verification
- [ ] Ensure QR cannot be trivially forged
- [ ] Ensure QR does not expose a guessable ticket identifier

## Definition of Done

- [ ] Paid order generates ticket
- [ ] Ticket contains required information
- [ ] QR code is generated
- [ ] QR payload is securely verifiable
- [ ] Forged or modified QR payloads are rejected

## Commit Target

`feat: generate digital tickets with signed qr codes`
'@
    },

    @{
        Title = "Day 11 - Check-In"
        Body = @'
## Goal

Implement secure event check-in using QR tickets.

## Checklist

- [ ] Implement QR scan/check-in endpoint
- [ ] Verify QR signature
- [ ] Verify ticket exists
- [ ] Verify ticket belongs to event
- [ ] Verify ticket status
- [ ] Record check-in timestamp
- [ ] Transition ticket to Checked In
- [ ] Prevent duplicate check-in
- [ ] Return explicit Already Checked In result
- [ ] Enforce Check-in Staff authorization
- [ ] Enforce tenant and event boundaries
- [ ] Handle simultaneous scans safely

## Definition of Done

- [ ] Authorized staff can check in valid tickets
- [ ] Invalid tickets are rejected
- [ ] Wrong-event tickets are rejected
- [ ] Duplicate scans do not create another check-in
- [ ] Check-in time is recorded

## Commit Target

`feat: implement secure ticket check-in`
'@
    },

    @{
        Title = "Day 12 - SignalR Real-Time Dashboard"
        Body = @'
## Goal

Provide organizers with live event metrics without manually refreshing.

## Checklist

- [ ] Create organizer dashboard metrics
- [ ] Implement Tickets Sold metric
- [ ] Implement Tickets Remaining metric
- [ ] Implement Checked In metric
- [ ] Configure SignalR
- [ ] Create appropriate hub
- [ ] Broadcast check-in updates
- [ ] Update dashboard without page refresh
- [ ] Restrict updates to the correct tenant
- [ ] Verify organizers cannot receive another tenant events

## Definition of Done

- [ ] Dashboard displays live metrics
- [ ] Check-in updates dashboard immediately
- [ ] No manual refresh required
- [ ] SignalR respects tenant isolation

## Commit Target

`feat: add real time organizer dashboard with signalr`
'@
    },

    @{
        Title = "Day 13 - RabbitMQ and Background Notifications"
        Body = @'
## Goal

Introduce asynchronous processing where it provides an actual architectural benefit.

## Checklist

- [ ] Configure RabbitMQ
- [ ] Define PaymentConfirmed event
- [ ] Publish event after verified payment
- [ ] Create ticket-processing consumer
- [ ] Create notification consumer
- [ ] Implement background email notification
- [ ] Ensure email does not block HTTP request
- [ ] Handle consumer failures
- [ ] Consider message processing idempotency
- [ ] Preserve tenant context
- [ ] Trace payment to message to consumers

## Definition of Done

- [ ] PaymentConfirmed event is published
- [ ] Multiple consumers process it independently
- [ ] Ticket processing is decoupled
- [ ] Notification processing is asynchronous
- [ ] RabbitMQ has a real architectural purpose

## Commit Target

`feat: decouple payment processing with rabbitmq`
'@
    },

    @{
        Title = "Day 14 - Observability, Reliability and Core Tests"
        Body = @'
## Goal

Make the system diagnosable and robust under failure scenarios.

## Checklist

- [ ] Configure structured logging with Serilog
- [ ] Add request context to logs
- [ ] Add payment-processing logs
- [ ] Add message-processing logs
- [ ] Add health checks
- [ ] Implement global exception handling
- [ ] Ensure sensitive information is not logged
- [ ] Test duplicate webhook scenario
- [ ] Test concurrent capacity scenario
- [ ] Test tenant isolation
- [ ] Test duplicate check-in
- [ ] Test payment failure scenarios
- [ ] Test notification failure behavior

## Definition of Done

- [ ] Structured logs are available
- [ ] Health endpoint works
- [ ] Global error handling works
- [ ] Critical architectural tests pass
- [ ] Important failure scenarios are covered

## Commit Target

`feat: add observability reliability and core tests`
'@
    },

    @{
        Title = "Day 15 - Docker Compose and CI"
        Body = @'
## Goal

Make the application reproducible locally and automatically verified through CI.

## Checklist

- [ ] Create Dockerfile
- [ ] Containerize API
- [ ] Configure SQL Server
- [ ] Configure Redis
- [ ] Configure RabbitMQ
- [ ] Create docker-compose.yml
- [ ] Configure service networking
- [ ] Configure environment variables and secrets
- [ ] Verify application starts through Compose
- [ ] Create GitHub Actions workflow
- [ ] Restore dependencies
- [ ] Build solution
- [ ] Run automated tests
- [ ] Make CI run on push
- [ ] Verify CI passes on main

## Definition of Done

- [ ] Infrastructure starts through Docker Compose
- [ ] API connects to dependencies
- [ ] CI builds the project
- [ ] CI runs tests
- [ ] Main branch is green

## Commit Target

`ci: containerize application and add github actions`
'@
    },

    @{
        Title = "Day 16 - Deployment, README Case Study and Final Polish"
        Body = @'
## Goal

Deploy the completed MVP and turn the repository into a professional backend case study.

## Deployment Checklist

- [ ] Prepare production configuration
- [ ] Deploy application
- [ ] Configure production infrastructure
- [ ] Configure required environment variables
- [ ] Verify application health
- [ ] Verify public API access
- [ ] Verify Swagger/OpenAPI is accessible
- [ ] Test core flow against deployed application

## README Checklist

- [ ] Replace placeholder README
- [ ] Add project overview
- [ ] Explain the business problem
- [ ] Explain multi-tenancy architecture
- [ ] Add architecture diagram
- [ ] Explain authentication and authorization
- [ ] Explain concurrency strategy
- [ ] Explain payment flow
- [ ] Explain webhook idempotency
- [ ] Explain signed QR tickets
- [ ] Explain RabbitMQ
- [ ] Explain SignalR
- [ ] Explain observability
- [ ] Explain Docker and CI/CD
- [ ] Document technical decisions
- [ ] Document trade-offs
- [ ] Document deferred features
- [ ] Add API setup instructions
- [ ] Add live Swagger/deployment link

## Final Verification

- [ ] Core business scenario works end-to-end
- [ ] Tenant isolation tests pass
- [ ] Capacity concurrency test passes
- [ ] Webhook idempotency test passes
- [ ] QR security works
- [ ] Duplicate check-in prevention works
- [ ] SignalR dashboard works
- [ ] RabbitMQ flow works
- [ ] Structured logging works
- [ ] Docker Compose works
- [ ] CI passes
- [ ] Production deployment works

## Definition of Done

The MVP acceptance checklist from PRD Section 29 is satisfied.

## Commit Target

`docs: finalize deployment and project case study`
'@
    }
)

foreach ($issue in $issues) {
    Write-Host "Creating: $($issue.Title)"

    gh issue create `
        --title $issue.Title `
        --body $issue.Body

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to create: $($issue.Title)" -ForegroundColor Red
        break
    }
}