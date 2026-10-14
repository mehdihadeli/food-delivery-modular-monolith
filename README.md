# 🌭 Food Delivery Modular Monolith

[![.NET](https://github.com/mehdihadeli/food-delivery-modular-monolith/actions/workflows/build-test.yml/badge.svg?branch=main&style=flat-square)](https://github.com/mehdihadeli/food-delivery-modular-monolith/actions/workflows/build-test.yml)


> `Food Delivery Modular Monolith` is a .NET 10 sample using **Modular Monolith Architecture**, **Vertical Slice Architecture**, **CQRS**, **Domain-Driven Design**, and **Event-Driven Architecture**. The public API dispatches requests into isolated module providers. Asynchronous integration events use one host-owned Wolverine runtime with PostgreSQL-backed durable local queues; synchronous lookups use module client APIs where needed.

💡 This application is not business-oriented and my focus is mostly on the technical part, I just want to implement a sample using different technologies, software architecture design, principles, and all the things we need for creating a modular monolith app.

This application ported to `microservices architecture` in another repository which is available in [food-delivery-microservices](https://github.com/mehdihadeli/food-delivery-microservices) repository.

🌀 Keep in mind this repository is a work in progress and will be completed over time 🚀

# ⭐ Support
If you like feel free to ⭐ this repository, It helps out :)

Thanks a bunch for supporting me!

# Table of Contents

- [🌭 Food Delivery Modular Monolith](#-food-delivery-modular-monolith)
- [⭐ Support](#-support)
- [Table of Contents](#table-of-contents)
  - [Plan](#plan)
  - [Technologies - Libraries](#technologies---libraries)
  - [The Domain And Bounded Context - Modules Boundary](#the-domain-and-bounded-context---modules-boundary)
  - [Application Architecture](#application-architecture)
    - [PostgreSQL-Backed In-Process Messaging](#postgresql-backed-in-process-messaging)
      - [Transaction Flow](#transaction-flow)
        - [Architecture Flow](#architecture-flow)
        - [Integration Event Routes](#integration-event-routes)
        - [Event Route Flow](#event-route-flow)
      - [Configuration And Deployment](#configuration-and-deployment)
      - [Delivery Limits](#delivery-limits)
  - [Application Structure](#application-structure)
    - [High Level Structure](#high-level-structure)
  - [Vertical Slice Flow in Modules](#vertical-slice-flow-in-modules)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Clone With Submodules](#clone-with-submodules)
    - [How to Run](#how-to-run)
  - [Contribution](#contribution)
  - [Project References](#project-references)
  - [License](#license)


## Plan

> This project is in progress, new features will be added over time.

High-level plan is represented in the table

| Feature | Status |
| ------- | ------ |
| Building Blocks | Squirrel submodule; integration evolves with the sample |

## Technologies - Libraries

- ✔️ **[`.NET 10`](https://dotnet.microsoft.com/download)** - ASP.NET Core API and module projects; SDK selection is defined in [global.json](global.json)
- ✔️ **[`Npgsql Entity Framework Core Provider`](https://www.npgsql.org/efcore/)** - Npgsql has an Entity Framework (EF) Core provider. It behaves like other EF Core providers (e.g. SQL Server), so the general EF Core docs apply here as well
- ✔️ **[`FluentValidation`](https://github.com/FluentValidation/FluentValidation)** - Popular .NET validation library for building strongly-typed validation rules
- ✔️ **[`Swagger & Swagger UI`](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)** - Swagger tools for documenting API's built on ASP.NET Core
- ✔️ **[`Serilog`](https://github.com/serilog/serilog)** - Simple .NET logging with fully-structured events
- ✔️ **[`Polly`](https://github.com/App-vNext/Polly)** - Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner
- ✔️ **[`Scrutor`](https://github.com/khellang/Scrutor)** - Assembly scanning and decoration extensions for Microsoft.Extensions.DependencyInjection
- ✔️ **[`Opentelemetry-dotnet`](https://github.com/open-telemetry/opentelemetry-dotnet)** - The OpenTelemetry .NET Client
- ✔️ **[`DuendeSoftware IdentityServer`](https://github.com/DuendeSoftware/IdentityServer)** - The most flexible and standards-compliant OpenID Connect and OAuth 2.x framework for ASP.NET Core
- ✔️ **[`Newtonsoft.Json`](https://github.com/JamesNK/Newtonsoft.Json)** - Json.NET is a popular high-performance JSON framework for .NET
- ✔️ **[`AspNetCore.Diagnostics.HealthChecks`](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)** - Enterprise HealthChecks for ASP.NET Core Diagnostics Package
- ✔️ **[`Microsoft.AspNetCore.Authentication.JwtBearer`](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer)** - Handling Jwt Authentication and authorization in .Net Core
- ✔️ **[`NSubstitute`](https://github.com/nsubstitute/NSubstitute)** - A friendly substitute for .NET mocking libraries.
- ✔️ **[`StyleCopAnalyzers`](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)** - An implementation of StyleCop rules using the .NET Compiler Platform
- ✔️ **[`AutoMapper`](https://github.com/AutoMapper/AutoMapper)** - Convention-based object-object mapper in .NET.
- ✔️ **[`Mediator`](https://github.com/martinothamar/Mediator)** - Source-generated command, query, and notification dispatch inside each module
- ✔️ **[`Wolverine`](https://wolverinefx.net/)** - Host-owned durable in-process integration messaging with PostgreSQL inbox/outbox
- ✔️ **ASP.NET Core Problem Details** - API error responses through the Squirrel Web integration
- ✔️ **[`IdGen`](https://github.com/RobThree/IdGen)** - Twitter Snowflake-alike ID generator for .Net


## The Domain And Bounded Context - Modules Boundary

The application has four modules: Catalogs manages products and stock; Identity manages users and authentication; Customers manages customer-related workflows and restock subscriptions; Orders contains order-related workflows and integration consumers. Each module owns its feature slices and dependencies. Module isolation is enforced by separate DI providers and business schemas, not separate operating-system processes or database servers.

## Application Architecture

The **public API** hosts module endpoints and shared infrastructure. Minimal endpoints use [GatewayProcessor](src/BuildingBlocks/src/Squirrel.Web/GatewayProcessor.cs) to dispatch commands and queries in the owning module's scope. Controllers use [CustomServiceBasedControllerActivator](src/Api/FoodDelivery.Api/CustomServiceBasedControllerActivator.cs). For a feature example, see [CreateProduct](src/Modules/Catalogs/FoodDelivery.Modules.Catalogs/Products/Features/CreatingProduct/CreateProduct.cs).

[ApplicationConfigurations](src/Api/FoodDelivery.Api/ApplicationConfigurations.cs) creates and owns the separate module providers. [CompositionRootRegistry](src/BuildingBlocks/src/Squirrel.Web/Module/CompositionRootRegistry.cs) records their composition roots; it does not create them. Modules retain their own mediators and DbContexts, while the host owns one Wolverine runtime and the bridges needed to dispatch integration events into those providers.

![Separate module composition roots](./assets/composition-roots.png)

Modules own separate PostgreSQL business schemas in the same database. This is logical ownership, not a security boundary against arbitrary SQL access. Shared Wolverine storage participates in module transactions. Extracting a module into a service would require changes to transport, persistence ownership, and transaction boundaries; separate DI providers alone do not make extraction automatic.

Asynchronous integration events use durable Wolverine local queues. Synchronous module lookups can use existing HTTP client abstractions when an immediate answer is needed. The current implementation does not require gRPC or RabbitMQ for module-to-module event delivery. Subscriber failures are retried independently and do not roll back an already committed producer transaction.

The application uses [CQRS](https://event-driven.io/en/cqrs_facts_and_myths_explained/) to separate command and query handling. It is not event-sourced: PostgreSQL stores business state, and events drive integration and projection updates rather than serving as the source of truth.

PostgreSQL provides transactional writes. MongoDB stores document-based read models for selected queries. Their performance depends on query patterns, indexes, and workload; using separate stores does not by itself guarantee faster reads or better scalability.

For syncing our read side and write side we have 2 options with using Event Driven Architecture (without using events streams in event sourcing):

- Within a module, internal commands update MongoDB read models through the module's mediator. These commands currently execute inline; they are not independently persisted background jobs.

- Between modules, integration events use Wolverine's PostgreSQL-backed transactional messaging. Subscribers run asynchronously in their own module scopes and can update their write and read models.

All of this is optional in the application and it is possible to only use what that the service needs. Eg. if the service does not want to Use DDD because of business is very simple and it is mostly `CRUD` we can use `Data Centric` Architecture or If our application is not `Task based` instead of CQRS and separating read side and write side again we can just use a simple `CRUD` based application.

### PostgreSQL-Backed In-Process Messaging

The API host owns one Wolverine 6.2 runtime with durable local queues. Each module keeps its own dependency injection provider, mediator, and database contexts. RabbitMQ is not required for this module-to-module path: transport runs in process, while PostgreSQL stores pending deliveries, handled envelopes, and dead letters in the `wolverine` schema. This uses Wolverine's built-in inbox/outbox, not custom messaging tables or a custom dispatcher.

#### Transaction Flow

1. A module command runs inside its existing EF transaction, normally through an `ITxRequest`/`ITxCommand` mediator pipeline. [ModuleEventBus](src/Api/FoodDelivery.Api/Messaging/ModuleEventBus.cs) enlists Wolverine's `DatabaseEnvelopeTransaction` in that same PostgreSQL transaction. Publishing without a transaction or without a registered route fails instead of silently losing the event.
2. Wolverine stores the routed envelope before the transaction commits. For durable local queues, the pending envelope can be stored directly in the native incoming-envelope table; a separate module outbox table is unnecessary. Business changes and message storage either commit together or roll back together.
3. [ModuleMessagingSession](src/Api/FoodDelivery.Api/Messaging/ModuleMessagingSession.cs) flushes outgoing messages only after EF reports a successful commit. If the process stops after commit but before flush, Wolverine's durability agents recover the persisted envelope when another runtime starts or takes ownership.
4. [IntegrationContractHandler](src/Api/FoodDelivery.Api/Messaging/IntegrationContractHandler.cs) maps producer CLR contracts to subscriber contracts. [ModuleMessageHandler](src/Api/FoodDelivery.Api/Messaging/ModuleMessageHandler.cs) opens a scope in the subscriber's provider and enlists Wolverine's native EF envelope transaction. Subscriber PostgreSQL changes, inbox completion, and further outgoing events commit together before transport completion.
5. Failed deliveries receive scheduled retries after 1, 5, and 30 seconds. Exhausted retries move the envelope to Wolverine's persistent dead-letter storage for investigation and selective replay. Wolverine manages node ownership and recovery across replicas sharing this database.

For example, Identity commits a user and `UserRegistered`; Customers consumes it in its own provider, commits the customer and `CustomerCreated`, and Orders receives that event. Module command buses and DbContexts are not registered in host DI. The host owns and disposes the module providers.

```mermaid
flowchart LR
  Api["Public API<br/>HTTP entry point"]
  Modules["Catalogs / Identity / Customers / Orders<br/>Separate DI providers<br/>Own mediator and EF DbContext"]
  Wolverine["One host-owned Wolverine runtime<br/>Durable local queues<br/>Module-scoped delivery"]
  Postgres[("Shared PostgreSQL<br/>Module business schemas<br/>Native Wolverine inbox / outbox<br/>Retries and dead letters")]
  Mongo[("MongoDB read models<br/>Non-transactional projections")]

  subgraph Host["Single API host process"]
    Api -->|Gateway dispatch| Modules
    Modules -->|Transactional messaging| Wolverine
  end
  Wolverine -->|Persist and recover| Postgres
  Modules -.->|Inline internal commands| Mongo

  classDef api fill:#e0f7ff,stroke:#0284c7,color:#111827,stroke-width:2px
  classDef module fill:#ffffff,stroke:#cf087b,color:#111827,stroke-width:2px
  classDef bus fill:#fff4c2,stroke:#b77900,color:#111827,stroke-width:2px
  classDef store fill:#e8f1f8,stroke:#35698a,color:#111827
  class Api api
  class Modules module
  class Wolverine bus
  class Postgres,Mongo store
```

##### Architecture Flow

1. **API to module:** An HTTP request enters the public API. The gateway dispatches the command or query to the owning module's provider, where its mediator and dependencies are resolved. The grouped module box represents four isolated providers, not one shared container.
2. **Module to PostgreSQL:** A transactional command saves business changes and its outgoing Wolverine envelopes in the same PostgreSQL transaction. The direct business-storage connection is omitted from the overview to keep routing readable. Rollback removes both the changes and pending events.
3. **Module to Wolverine:** After commit, the module adapter flushes the persisted events to the host-owned durable local queues. Delivery is asynchronous; committing the producer transaction does not mean every subscriber has finished.
4. **Wolverine to subscriber:** Wolverine maps the event contract and opens a scope in the receiving module. Subscriber PostgreSQL changes, inbox completion, and any further outgoing envelopes commit together. Persisted pending work survives restart; exhausted retries move to dead-letter storage.
5. **Module to MongoDB:** Inline internal commands update MongoDB read models. The dashed projection path is outside PostgreSQL's transaction, so a MongoDB update cannot be rolled back with PostgreSQL and needs idempotency or reconciliation after failure.

##### Integration Event Routes

Each arrow below is an asynchronous route through the same Wolverine runtime, not a direct module call. Events become available only after producer commit; subscribers run in their own module scopes.

```mermaid
flowchart LR
  Identity["Identity"]
  Catalogs["Catalogs"]
  Customers["Customers"]
  Orders["Orders"]

  Identity -->|UserRegistered| Customers
  Catalogs -->|"ProductCreated<br/>ProductStockReplenished"| Customers
  Customers -->|CustomerCreated| Orders

  classDef module fill:#ffffff,stroke:#cf087b,color:#111827,stroke-width:2px
  class Catalogs,Identity,Customers,Orders module
  linkStyle default stroke:#16a34a,color:#166534,stroke-width:2px
```

##### Event Route Flow

1. **Identity to Customers:** Identity registers a user and commits `UserRegistered` with its business changes. Wolverine delivers the mapped contract to Customers, whose handler creates the corresponding customer in its own module scope.
2. **Catalogs to Customers:** Catalogs publishes `ProductCreated` or `ProductStockReplenished` after its transaction commits. Customers receives the mapped product event to perform its product-related processing independently of the original catalog request.
3. **Customers to Orders:** Creating a customer produces `CustomerCreated`. Its envelope commits with the Customers transaction, then Wolverine delivers the mapped contract to the Orders consumer. This is a new delivery with its own completion state, not part of Identity's original transaction.
4. **Failure and recovery:** Each hop uses the same durable Wolverine runtime; arrows do not represent direct calls between module providers. A failed subscriber transaction rolls back its PostgreSQL changes and outgoing envelopes. Wolverine retries the delivery, then retains exhausted failures as dead letters for selective replay. Previously committed producer changes remain committed.

#### Configuration And Deployment

The messaging connection is selected from `WolverineInMemoryBusOptions:DurabilityConnectionString`, then `ConnectionStrings:wolverine`, then `Identity:PostgresOptions:ConnectionString`. All four module connections must target the same PostgreSQL host, port, and database as Wolverine. Startup rejects mismatches because a transaction cannot atomically update two independent databases. Module schemas and DI providers remain separate; this is not database-level tenant isolation.

Supply deployment connection strings through configuration or prefixed environment variables such as `food_delivery_env_ConnectionStrings__wolverine`. Each module's database role needs access to the Wolverine schema used by its transaction. Do not commit production credentials.

Development and test startup create/update Wolverine storage automatically. Production disables automatic message-storage migration. Apply and verify schema changes during deployment, with production configuration and a suitably privileged deployment identity:

```powershell
dotnet run --project src/Api/FoodDelivery.Api --no-launch-profile -- db-apply
dotnet run --project src/Api/FoodDelivery.Api --no-launch-profile -- db-assert
```

For a published deployment, run the equivalent `dotnet FoodDelivery.Api.dll db-apply` and `db-assert` commands from its deployment directory. These maintenance commands do not start the API or run module startup migrations. Business-schema migrations retain their existing module startup behavior.

After correcting a failed dependency or handler, replay only the affected dead-letter IDs through Wolverine's administration API:

```csharp
using Wolverine.Persistence.Durability.DeadLetterManagement;
using Wolverine.Runtime;

var runtime = services.GetRequiredService<IWolverineRuntime>();
await runtime.Storage.DeadLetters.ReplayAsync(
    new DeadLetterEnvelopeQuery { MessageIds = [envelopeId] }, cancellationToken);
```

Replay is an administrative operation, not a public unauthenticated endpoint. Monitor pending-envelope age, retries, dead letters, database availability, and handler failures. Back up message storage with the business database. Do not run destructive storage clear/rebuild commands on production queues, or release all ownership while nodes are running.

#### Delivery Limits

- Delivery is **at least once**, not globally exactly once. Wolverine generates and persists a stable envelope ID; deduplication does not use the shared `Message.MessageId` getter, which currently generates a fresh ID on every access. Handled envelopes are retained for 24 hours. Republishing the same business event creates a new envelope, and retries after retention expiry still require business-level idempotency.
- MongoDB writes, HTTP calls, email, and inline internal commands do not share PostgreSQL's transaction. They can succeed before a later PostgreSQL rollback. Make these effects idempotent and provide projection rebuild/reconciliation; this implementation does not make them exactly once or independently durable.
- Current integration mappings have one subscriber per producer contract. Adding independently processed subscribers requires separate durable routing/queues and appropriate message identity, rather than assuming one inbox acknowledgement tracks every subscriber.
- Keep serialized event contracts compatible with pending and dead-letter messages during upgrades. Protect stored payloads and backups because events can contain customer data. Recovery depends on PostgreSQL durability and availability; adding a broker alone does not improve transaction guarantees.

Container-backed integration tests verify transactional rollback, normal Identity-to-Customers delivery, recovery of a committed unflushed envelope in a fresh runtime, and dead-letter replay retaining its original envelope ID.

## Application Structure

The modules use [vertical slice architecture](https://jimmybogard.com/vertical-slice-architecture/) and feature folders. Each use case groups its endpoint, request and response types, validation, and handler near each other rather than distributing them across application-wide technical layers.

![Layered architecture compared with vertical slices](./assets/vertical-slice-architecture.jpg)

![Vertical slice feature organization](./assets/vsa2.png)

Commands and queries have separate handlers. The source-generated Mediator pipeline supplies shared validation, logging, caching, and transaction behavior. A feature can choose its own query or storage approach while reusing those behaviors.

This organization aims to reduce coupling between features, not eliminate it. Changes to shared domain models, database schemas, integration contracts, or pipeline behavior can affect multiple slices. CQRS and feature folders do not guarantee higher performance or prevent breaking changes; those properties still depend on implementation and testing.

### High Level Structure

| Location | Responsibility |
| --- | --- |
| [src/Api/FoodDelivery.Api](src/Api/FoodDelivery.Api) | HTTP host, module-provider ownership, gateway dispatch, and Wolverine bridges |
| [src/Modules](src/Modules) | Catalogs, Customers, Identity, and Orders feature slices and persistence |
| [src/BuildingBlocks](src/BuildingBlocks) | Pinned Squirrel Git submodule containing shared infrastructure |
| [src/Aspire](src/Aspire) | AppHost and service-defaults projects for orchestration |
| [tests](tests) | Module tests and shared container-backed fixtures |

## Vertical Slice Flow in Modules

An HTTP endpoint dispatches a command or query through the gateway into its module's mediator. Validation and other pipeline behaviors run before the feature handler. Transactional requests enlist integration events with their PostgreSQL writes; commit releases those events for asynchronous delivery. Queries return the module's response without opening a producer transaction. Internal commands may update MongoDB projections inline, outside PostgreSQL atomicity.

## Getting Started

### Prerequisites

1. Install [Git](https://git-scm.com/downloads).
2. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). [global.json](global.json) selects SDK `10.0.100` with `latestFeature` roll-forward.
3. Install Docker with a running engine and support for Linux containers. Integration tests use Testcontainers.
4. Use VS Code, Rider, or another IDE with .NET 10 support.
5. Configure an HTTPS development certificate if using the API's HTTPS launch profile.

### Clone With Submodules

Clone the project together with its Git submodules. The `--recurse-submodules` option automatically initializes the Squirrel BuildingBlocks source under `src/BuildingBlocks`:

```powershell
git clone --recurse-submodules https://github.com/mehdihadeli/food-delivery-modular-monolith.git
cd food-delivery-modular-monolith
```

After cloning, verify that the submodule is initialized:

```powershell
git submodule status
```

If you already cloned without submodules, run this from the repository root:

```powershell
git submodule update --init --recursive
```

### How to Run

Run these PowerShell commands from the repository root after cloning with submodules. They use the Compose file's default development credentials and ports; adjust the connection strings if you override those settings.

1. Start PostgreSQL and MongoDB with [the infrastructure Compose file](deployments/docker-compose/docker-compose.infrastructure.yaml):

  ```powershell
  docker compose -f deployments/docker-compose/docker-compose.infrastructure.yaml up -d --wait postgres mongo
  ```

1. Create the business database on a fresh PostgreSQL instance. Skip `createdb` if `food_delivery` already exists. If `pg_isready` reports that PostgreSQL is still starting, retry it before creating the database:

  ```powershell
  docker compose -f deployments/docker-compose/docker-compose.infrastructure.yaml exec -T postgres pg_isready -U postgres
  docker compose -f deployments/docker-compose/docker-compose.infrastructure.yaml exec -T postgres createdb -U postgres food_delivery
  ```

1. Match MongoDB authentication to Compose and disable SMTP for local use. These overrides apply only to the current terminal session:

  ```powershell
  foreach ($module in 'Catalogs', 'Customers', 'Identity', 'Orders') {
     Set-Item "Env:food_delivery_env_${module}__MongoOptions__ConnectionString" 'mongodb://admin:admin@localhost:27017/?authSource=admin'
     Set-Item "Env:food_delivery_env_${module}__EmailOptions__Enable" 'false'
  }
  ```

1. Build and start the API. Development startup applies module migrations and initializes Wolverine storage:

  ```powershell
  dotnet build src/Api/FoodDelivery.Api
  dotnet run --project src/Api/FoodDelivery.Api --launch-profile FoodDelivery.Api.Http --no-build
  ```

1. Open [Swagger UI](http://localhost:5000/swagger). Request examples are in [_httpclients](_httpclients), usable with the VS Code [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) extension. Match each request's base URL to the selected launch profile.

To test email delivery, configure your own SMTP account under each module's `<Module>:EmailOptions` settings and enable email again. [Ethereal](https://ethereal.email/) captures test messages rather than delivering them to recipients; inspect them through your own account. Supply credentials through environment variables or a local secret store, not committed configuration. Existing sample credentials are not a supported setup dependency.

## Contribution
The application is in development status. You are feel free to submit pull request or create the issue.

##  Project References
- [https://github.com/kgrzybek/modular-monolith-with-ddd](https://github.com/kgrzybek/modular-monolith-with-ddd)
- [https://github.com/oskardudycz/EventSourcing.NetCore](https://github.com/

## License
The project is under [MIT license](https://github.com/mehdihadeli/food-delivery-modular-monolith/blob/main/LICENSE).
