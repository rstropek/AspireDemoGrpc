# AspireDemoGrpc

Opinionated sample demonstrating a multi-runtime, polyglot .NET Aspire distributed app composed of:

* .NET Aspire AppHost orchestrating projects, containers, Rust process, npm frontend – single `dotnet run --project AspireDemoGrpc.AppHost/` bootstrap
* gRPC server (.NET) + Web API (.NET minimal APIs) with service discovery and OpenTelemetry defaults
* MongoDB (with Mongo Express) provisioned & wired by Aspire resource builder
* MQTT broker (Eclipse Mosquitto) container + dynamic endpoint propagation via environment + example MQTT publish / SSE consume pattern
* Rust sidecar HTTP service participating as a first‑class Aspire resource
* Angular frontend consuming Web API (and indirectly gRPC, MQTT, Rust, MongoDB)
* Shared `.proto` contract compiled into both server and client gRPC types

The focus is how Aspire coordinates heterogeneous pieces (projects, containers, external processes) and injects configuration (endpoints, environment variables, connection strings) without bespoke glue code.

## Quick start

```
# From repo root
 dotnet run --project AspireDemoGrpc.AppHost/
```

The AppHost will build & launch: gRPC server, Web API (2 replicas), Rust app, Angular dev server, MongoDB + Mongo Express, Mosquitto broker. Endpoints are surfaced via Aspire dashboard.

## Topology (logical view)

```
[ Angular Frontend ] --HTTP--> [ WebApi (replicas) ] --gRPC--> [ GrpcServer ]
          |                          |  \--HTTP--> [ Rusty (Rust) ]
          |                          |  \--MongoDB driver--> [ MongoDB ] (+ Mongo Express UI)
          |                          |  \--MQTT publish/subscribe--> [ Mosquitto ]
          |<--SSE /messages----------| (bridged from MQTT topic)
```

## AppHost (`AspireDemoGrpc.AppHost`)

`AppHost.cs` uses `DistributedApplication.CreateBuilder` to declare resources declaratively:

* `AddProject<GrpcServer>` – gRPC service (`Greeter`)
* `AddContainer("mosquitto")` – Eclipse Mosquitto with bound config and custom TCP endpoint metadata (`mqtt`)
* `AddMongoDB("mongodb").WithDataVolume().WithMongoExpress()` – database plus admin UI; adds a logical database resource consumed by Web API
* `AddRustApp("rusty")` – compiles & runs the Rust workspace (HTTP endpoint bound from env `PORT`)
* `AddProject<WebApi>` – references gRPC server, Mongo database, Rust app, and Mosquitto endpoint; sets replica count to 2; injects MQTT host/port environment variables
* `AddNpmApp("frontend")` – Angular application with external HTTP exposure (reverse proxy / direct access)

Aspire handles build ordering (`WithReference`, `WaitFor`) and propagates:

* Connection strings (MongoDB)
* Service endpoints (Rust, gRPC, Mosquitto) via configuration hierarchy `Services:<name>:<endpoint>:<index>`
* Environment variables (`MQTT__HOST`, `MQTT__PORT`) for explicit consumption in Web API

## Service Defaults (`AspireDemoGrpc.ServiceDefaults`)

Imported via `builder.AddServiceDefaults()` in services, centralizing cross‑cutting concerns (logging, tracing, health endpoints) through Aspire conventions. Each service calls `app.MapDefaultEndpoints()` enabling health, liveness, readiness + telemetry wiring without custom code.

## gRPC Layer (`GrpcServer`, `GrpcShared`)

* Proto contract: `GrpcShared/greet.proto` (package `Demo`, service `Greeter`)
* Server registers `GreeterService` and CORS policy exposing gRPC metadata headers for browser gRPC-Web or grpcui tooling.
* Client in Web API adds `Greeter.GreeterClient` pointing to logical address `http://grpcserver` (service discovery name resolves through Aspire network).

## Web API (`WebApi`)

Minimal API acting as an integration façade:

* `/ping` – health style sample
* `/ip` – demonstrates resolving container service endpoint (Mosquitto) via injected configuration (`Services:mosquitto:mqtt:0`) and DNS lookup
* `/callGreeter` – gRPC client call wrapped in an ActivitySource for OpenTelemetry tracing
* MQTT endpoints (see below)
* User management & Rust integration extension endpoints (`UseUserManagement`, `UseRustyEndpoints`) – illustrating modular capability composition

### MQTT + SSE bridging

`MqttMessagingExtensions` exposes:

* `POST /sendMessage` – publishes JSON body `message` to MQTT topic `demo/messages`
* `GET /messages` – Server-Sent Events stream subscribing to same topic; each MQTT publication pushes an SSE event to browser clients

Environment variables `MQTT__HOST`, `MQTT__PORT` are injected by AppHost so no hardcoded broker coordinates.

### MongoDB

`builder.AddMongoDBClient("mongodbdatabase")` wires a typed client (connection string derived from resource) – additional collection / repo code can be added without altering infrastructure wiring.

### Rust Sidecar (`rusty`)

Declared as a first-class resource; Web API extension `UseRustyEndpoints` (see `RustyEndpointsExtensions.cs`) likely proxies / composes the Rust service responses, showcasing cross-language service boundaries.

## Frontend (`Frontend` – Angular)

Added via `AddNpmApp`; Aspire builds (npm install/build) and runs dev server with an HTTP endpoint (env `PORT`). References Web API resource so base URL is injected (via service discovery) – the frontend can call `/callGreeter`, `/messages` SSE, domain endpoints etc., without manual configuration drift.

## Mosquitto (`Mosquitto/mosquitto.conf`)

Containerized broker configuration is bind-mounted so local edits immediately affect runtime; Aspire reuses host file system for rapid iteration. Endpoint published as logical `mqtt` enabling environment variable injection and optional exposure.

## Observability

Service defaults imply:

* OpenTelemetry tracing & metrics (ActivitySource used explicitly in `/callGreeter`)
* Standard health endpoints surfaced by `MapDefaultEndpoints()`
* Replica count for Web API enables observing load-balancing / multi-instance tracing correlation

You can layer in the Aspire dashboard (`dotnet run` with dashboard enabled via environment or project settings) to visualize resource health, logs, traces, and connection metadata.

## Repo Layout Overview

```
AspireDemoGrpc.sln
├─ AspireDemoGrpc.AppHost/        # AppHost orchestrator (primary entrypoint)
├─ AspireDemoGrpc.ServiceDefaults/# Centralized logging/tracing/health defaults
├─ GrpcShared/                    # .proto contract shared across services
├─ GrpcServer/                    # gRPC Greeter implementation
├─ WebApi/                        # Minimal API gateway / integration layer
├─ Frontend/                      # Angular client (served via npm dev server)
├─ Mosquitto/                     # Broker config bind-mounted into container
├─ rusty/                         # Rust sidecar service (HTTP)
└─ (MongoDB, Mongo Express, Mosquitto containers created at runtime)
```

## Why This Sample

Shows how .NET Aspire unifies heterogeneous components with minimal bespoke orchestration code. Focus is on declarative resource graph, automatic config propagation, and polyglot integration versus raw infrastructure scripting.
