[English](README.md) | [Русский](README.ru.md)

# AquaSmart IoT Platform

![.NET 8](https://img.shields.io/badge/.NET_8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL_16-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![gRPC](https://img.shields.io/badge/gRPC-244C5A?style=for-the-badge&logo=grpc&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![C++](https://img.shields.io/badge/C++_ESP32-00599C?style=for-the-badge&logo=c%2B%2B&logoColor=white)

> **A distributed IoT ecosystem for real-time monitoring and automation of aquariums, terrariums, and growboxes.**  
> This project is designed as an engineering portfolio piece to demonstrate advanced software architecture, distributed systems communication, comprehensive automated testing, and hardware-software integration.

---

## Architectural Overview

The backend is built as a distributed system using a **Microservices Architecture**. It is designed for high availability, fault tolerance, and strict separation of concerns.

### Microservices Breakdown
*   **API Gateway (YARP)**: Acts as the single entry point. Handles request routing, SSL termination, and centralized JWT validation.
*   **Identity Service**: Manages users, JWT authentication, refresh token rotation, and Role/Subscription-based access control (RBAC).
*   **Device Service**: Handles hardware node provisioning, configuration syncing, and relay command queuing. Acts as a gRPC server for hardware validation.
*   **Control Service**: Evaluates automation rules, schedules (Cron-based via Quartz), and manages vacation modes.
*   **Telemetry Service**: Ingests high-frequency sensor data, aggregates it to optimize database storage, and broadcasts real-time updates via SignalR.
*   **Notification Service**: Processes alerts and handles asynchronous message delivery via Telegram API and SMTP.

---

## Inter-Service Communication

To ensure decoupling and performance, the system employs a hybrid communication strategy:

1.  **Asynchronous Messaging (RabbitMQ & MassTransit)**
    Used for eventual consistency and event-driven reactions (e.g., `SensorNoDataEvent`, `TelemetryReceivedEvent`). To solve the dual-write problem, the **Transactional Outbox Pattern** is implemented. Domain events are serialized into an `outbox_messages` table within the same EF Core transaction and subsequently dispatched by a background Quartz worker.
2.  **Synchronous RPC (gRPC)**
    Used exclusively for strict, high-performance internal validations that cannot rely on eventual consistency. Defined via strict `.proto` contracts (e.g., `DeviceIntegrationGrpc`). For instance, the Control and Notification services query the Device service via gRPC to validate hardware token assignments and fetch metadata before processing rules.
3.  **Real-Time Streaming (SignalR)**
    Used in the Telemetry Service to stream raw and aggregated sensor data directly to connected web or mobile clients.

---

## Testing Strategy

The platform features a rigorous, multi-layered automated testing pipeline, orchestrated via **GitHub Actions** (`microservices-ci.yml`).

<details>
<summary><b>View Testing Layers Details</b></summary>

*   **Architecture Tests (`NetArchTest.Rules`)**
    Ensures strict adherence to Clean Architecture. Tests verify that Domain layers have no external dependencies, application layers only depend on Domain, and that all Handlers/Validators follow project naming conventions.
*   **Unit Tests (`xUnit`, `NSubstitute`, `FluentAssertions`)**
    Isolates and tests domain entities, value objects, MediatR pipeline behaviors (e.g., `TransactionBehavior`), and business logic without invoking the database.
*   **Integration Tests (`Testcontainers`, `Respawn`)**
    Spins up ephemeral PostgreSQL containers for each test run. Validates EF Core mappings, Dapper queries, and MediatR handlers interacting with the actual database. `Respawn` is used to cleanly reset database states between test executions.
*   **End-to-End (E2E) Tests (`WebApplicationFactory`)**
    Validates complete HTTP request lifecycles, including API Gateway routing, JWT authorization handlers, and HTTP response serialization. Mocked authentication handlers bypass actual token generation to focus on endpoint behavior.
</details>

---

## Applied Patterns and Principles

The codebase strictly adheres to **SOLID** principles and heavily utilizes modern enterprise design patterns.

### 1. Domain-Driven Design (DDD)
*   **Rich Domain Models**: Entities (e.g., `Sensor`, `Relay`) encapsulate their own state. State mutation is only possible through explicit domain methods, protecting invariants.
*   **Value Objects**: Concepts like `MacAddress`, `ConnectionAddress`, and `ConditionThreshold` are modeled as immutable records with built-in validation.

### 2. CQRS (Command Query Responsibility Segregation)
*   **Commands (Write)**: Handled via Entity Framework Core 8 to leverage change tracking and complex domain logic.
*   **Queries (Read)**: Handled via **Dapper**. Raw SQL queries bypass EF Core overhead, directly projecting database views into lightweight DTOs for maximum performance.
*   **Pipeline Behaviors**: Cross-cutting concerns (Logging, Validation via `FluentValidation`, Transactions, and Security Authorization) are injected seamlessly via MediatR behaviors.

### 3. Result Pattern (Railway-Oriented Programming)
Exceptions are not used for business logic flow. A custom `Result<T>` and `Error` pattern is utilized across all layers, translating strongly-typed domain errors into standardized HTTP responses.

---

## Embedded Systems Architecture (ESP32 / C++)

The firmware running on the ESP32 microcontroller is engineered for fault tolerance in unstable network environments.

*   **Finite State Machine**: The core logic operates on a state machine (`BOOT`, `CONNECT_WIFI`, `ONLINE`, `DEGRADED_OFFLINE`, `SAFE_MODE`).
*   **Offline Resilience (Circular Buffer)**: Implemented a custom `TelemetryQueue` allocated in IRAM. If the network drops, data is queued locally. Once restored, the batch is flushed to the server.
*   **Hardware Fail-Safes**: Critical thresholds trigger local hardware interrupts to shut down high-voltage relays, completely bypassing the backend to ensure physical safety.

---

## Technology Stack

| Category | Technologies |
| :--- | :--- |
| **Framework** | .NET 8, ASP.NET Core Web API |
| **Data Access** | Entity Framework Core 8, Dapper, Npgsql (PostgreSQL 16) |
| **Message Broker** | RabbitMQ, MassTransit |
| **RPC & Real-Time** | gRPC, Protobuf, SignalR |
| **Background Jobs** | Quartz.NET |
| **Testing** | xUnit, Testcontainers, Respawn, NSubstitute, FluentAssertions, NetArchTest |
| **Observability** | Serilog, Elasticsearch, Kibana (ELK) |
| **Gateway & Auth** | YARP, BCrypt.Net, JWT Bearer |
| **Embedded / Edge** | PlatformIO, C++, FreeRTOS |
| **Infrastructure** | Docker, Docker Compose, GitHub Actions |

---

## Local Deployment

The entire backend infrastructure is fully containerized and can be launched with a single command. 

**Prerequisites**: Docker Desktop

Clone the repository:
   ```bash
   git clone https://github.com/romansevryuk375/aquasmart-iot-platform.git
   cd aquasmart-iot-platform
   ```
Start the infrastructure (Databases, RabbitMQ, ELK Stack, Microservices):
    ```bash
    docker compose up --build -d
    ```
Access the endpoints:

        API Gateway: http://localhost:5055

        RabbitMQ Management: http://localhost:15672 (guest/guest)

        Kibana (Logs): http://localhost:5601

Entity Framework migrations are applied automatically upon container startup via DatabaseMigrationService.
