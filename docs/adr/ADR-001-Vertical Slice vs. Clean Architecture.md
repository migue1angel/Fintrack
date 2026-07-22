# ADR-001: [Clean Architecture vs Vertical Slice]

**Fecha:** 2026-03-19  
**Estado:** Aceptado 

## Contexto

FinTrack is being developed as a project focused on:

- Serverless architecture
- AWS Lambda
- Event-Driven Architecture
- CQRS
- Vertical Slice Architecture
- Future extraction of independent services

The initial implementation will start as a modular monolith,
but the long-term goal is to evolve parts of the system into
independent serverless workloads.

The main architectural alternatives considered were:
- Clean Architecture
- Vertical Slice Architecture

## Decisión

Since this project is planned to become a serverless system where features 
are decoupled and independent.  VSA provides  stronger boundaries around 
individual capabilities and simplifies future extraction into independent 
serverless workloads.

The decision was  driven primarily by requirements.
Since the application is expected to evolve into a serverless architecture, VSA
provides a better foundation for future feature extraction and independent
deployment.
Each module is organized as follows: 
- Features, entities, Module.cs
- Each feature should contain: Command, Handler, Validator
```
src/
│   │   │   ├── Modules/
│   │   │   │   ├── Users/
│   │   │   │   │   ├── Features/
│   │   │   │   │   │   ├── Register/
│   │   │   │   │   │   │   ├── RegisterCommand.cs
│   │   │   │   │   │   │   ├── RegisterHandler.cs
│   │   │   │   │   │   │   └── RegisterCommandValidator.cs
│   │   │   │   │   │   ├── Login/
│   │   │   │   │   │   └── GetMe/
│   │   │   │   │   ├── Entities/
│   │   │   │   │   │   └── UserEntity.cs
│   │   │   │   │   └── Users.Module.cs  ← registro de DI del módulo
│   │   │   │   │
```

## Consecuencias

### Positivas
- Simplifies an eventual migration towards a serverless system with Lambda
- Fast on-board for new developers

### Negativas / trade-offs asumidos
- Developers must actively monitor and manage potential code duplication across
slices.

### Cosas que deberemos resolver si la decisión resulta incorrecta
- If future requirements introduce highly complex domain logic,
  the team should reevaluate whether additional architectural
  patterns are required within specific slices.