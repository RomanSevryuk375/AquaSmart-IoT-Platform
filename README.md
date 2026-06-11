# AquaSmart IoT Platform

![.NET 8](https://img.shields.io/badge/.NET_8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL_18-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![C++](https://img.shields.io/badge/C++_ESP32-00599C?style=for-the-badge&logo=c%2B%2B&logoColor=white)

> **A distributed IoT ecosystem for real-time monitoring and automation of aquariums, terrariums, and growboxes.**  
> This project is designed as an engineering portfolio piece to demonstrate advanced software architecture, distributed systems communication, and hardware-software integration.

---

## Architectural Overview

The backend is built as a distributed system using a **Microservices Architecture**. It is designed for high availability, fault tolerance, and strict separation of concerns.

### Microservices Breakdown
*   **API Gateway (YARP)**: Acts as the single entry point. Handles request routing, SSL termination, and centralized JWT validation.
*   **Identity Service**: Manages users, JWT authentication, refresh token rotation, and Role/Subscription-based access control (RBAC).
*   **Device Service**: Handles hardware node provisioning, configuration syncing, and relay command queuing.
*   **Control Service**: Evaluates automation rules, schedules (Cron-based via Quartz), and manages vacation modes.
*   **Telemetry Service**: Ingests high-frequency sensor data, aggregates it to optimize database storage, and broadcasts real-time updates via SignalR.
*   **Notification Service**: Processes alerts and handles asynchronous message delivery via Telegram API and SMTP.

---

## Applied Patterns and Principles

The codebase strictly adheres to **SOLID** principles and heavily utilizes modern enterprise design patterns.

### 1. Domain-Driven Design (DDD)
*   **Rich Domain Models**: Entities (e.g., `SensorEntity`, `RelayEntity`) encapsulate their own state. State mutation is only possible through explicit domain methods, protecting invariants.
*   **Aggregate Roots**: Implemented `AggregateRoot` base classes that track `IDomainEvent` notifications.
*   **Domain Events**: Changes in state emit events (e.g., `SensorNoDataDomainEvent`), which are mapped to integration events for inter-service communication.

### 2. CQRS (Command Query Responsibility Segregation)
*   Implemented using **MediatR**. 
*   Read operations (Queries) and Write operations (Commands) are handled in strict isolation.
*   **MediatR Pipeline Behaviors**: Cross-cutting concerns are extracted into pipeline behaviors:
    *   `ValidationBehavior`: Integrates with `FluentValidation` to short-circuit invalid requests.
    *   `TransactionBehavior`: Automatically wraps commands in an EF Core database transaction.
    *   `SecurityBehavior`: Enforces tenant isolation (ensuring users only mutate their own hardware entities).

### 3. Transactional Outbox Pattern
To guarantee eventual consistency across microservices and solve the dual-write problem, the Outbox pattern is implemented natively:
*   An EF Core `SaveChangesInterceptor` (`ConvertDomainEventsToOutboxMessagesInterceptor`) catches Domain Events right before the transaction commits, serializes them, and saves them to an `outbox_messages` table in the exact same transaction.
*   A robust **Quartz.NET** background worker constantly polls the outbox and publishes the messages to **RabbitMQ** via **MassTransit**, ensuring at-least-once delivery semantics.

### 4. Result Pattern (Railway-Oriented Programming)
Exceptions are not used for business logic flow. Instead, a custom `Result<T>` and `Error` pattern is utilized across all layers.
*   The domain layer returns strongly-typed errors (e.g., `Error.Validation`, `Error.NotFound`).
*   An extension method (`ToActionResult`) at the presentation layer automatically translates these domain errors into appropriate HTTP status codes (400, 404, 409), keeping controllers extremely thin.

### 5. GoF Design Patterns
*   **Strategy Pattern**: Used in the `TelemetryService` to evaluate dynamic automation rules (`EqualConditionEvaluator`, `GreaterConditionEvaluator`).
*   **Factory Method**: `RuleEvaluatorFactory` and `RuleOperatorFactory` instantiate the correct evaluation strategies based on user-defined configurations.
*   **Repository & Unit of Work**: Abstracts EF Core `DbContext`, providing a unified interface for data access and transaction management.

---

## Embedded Systems Architecture (ESP32 / C++)

The firmware running on the ESP32 microcontroller is engineered for fault tolerance in unstable network environments.

*   **Finite State Machine**: The core logic operates on a state machine (`SystemState::BOOT`, `CONNECT_WIFI`, `ONLINE`, `DEGRADED_OFFLINE`, `SAFE_MODE`).
*   **Offline Resilience (Circular Buffer)**: Implemented a custom `TelemetryQueue` allocated in IRAM. If the Wi-Fi or API goes down, the node continues to collect sensor data and switches to `DEGRADED_OFFLINE`. Once the connection is restored, the queued telemetry batch is flushed to the server.
*   **Hardware Fail-Safes**: Critical thresholds (e.g., water level sensors) trigger hard-coded hardware interrupts to shut down high-voltage relays locally, completely bypassing the backend to ensure physical safety.

---

## Technology Stack

| Category | Technologies |
| :--- | :--- |
| **Framework** | .NET 8, ASP.NET Core Web API |
| **Data Access** | Entity Framework Core 8, Npgsql (PostgreSQL 18) |
| **Message Broker** | RabbitMQ, MassTransit |
| **Background Jobs** | Quartz.NET |
| **Real-Time Comm.** | SignalR |
| **Validation** | FluentValidation |
| **Authentication** | BCrypt.Net, JWT Bearer |
| **Gateway** | YARP (Yet Another Reverse Proxy) |
| **Embedded / Edge** | PlatformIO, C++, FreeRTOS |
| **Infrastructure** | Docker, Docker Compose |

---

## Local Deployment

The entire backend infrastructure is fully containerized and can be launched with a single command. 

**Prerequisites**: [Docker Desktop](https://www.docker.com/products/docker-desktop/)

1. Clone the repository:
   ```bash
   git clone https://github.com/YourUsername/romansevryuk375-aquaapi.git
   cd romansevryuk375-aquaapi
   ```
2. Start the infrastructure (Databases, RabbitMQ, Microservices):
   ```bash
    docker compose up --build -d
   ```
Access the endpoints:
        API Gateway: http://localhost:5055
        RabbitMQ Management: http://localhost:15672 (guest/guest)

Note: Entity Framework migrations are automatically applied on container startup.
