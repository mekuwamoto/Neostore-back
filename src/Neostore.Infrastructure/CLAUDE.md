# Neostore.Infrastructure

Cross-cutting concerns and external service integrations. Currently a stub — only `DependencyInjection.cs` exists.

## Purpose

Intended for services that cross layer boundaries or integrate with external systems:
- AWS S3 (image upload/download)
- Email sending
- External auth providers
- Background jobs

## Current State

`DependencyInjection.cs` exposes `AddInfrastructure(IServiceCollection)` — returns `services` unchanged. No services registered yet.

## Rules

- Depends only on `Neostore.Domain` — no reference to Application or Persistence.
- Implements interfaces defined in Domain or Application — never defines its own contracts.
- Register all services in `DependencyInjection.cs`, not in `Startup.cs`.

## Adding a New Service

1. Define the interface in `Neostore.Domain/Interfaces/` (or `Neostore.Application/` if use-case specific).
2. Implement in `Neostore.Infrastructure/Services/`.
3. Register in `AddInfrastructure()`.
