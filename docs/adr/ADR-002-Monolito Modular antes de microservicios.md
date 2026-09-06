# ADR-NNN: [Monolito Modular antes de microservicios]

**Estado:**  Aceptado 

## Contexto

The long-term goal of FinTrack is to evolve into a set of
independent serverless workloads.

However, the domain boundaries between modules are still being
validated and may evolve as the system grows.

Starting directly with microservices would introduce distributed
system complexity before there is enough evidence that independent
deployment, scaling, or ownership is required.

## Decisión

The system will initially be implemented as a Modular Monolith.

Modules will be isolated through clear boundaries and internal
contracts while sharing the same deployment unit.

Data access will remain modular, although the initial implementation
will use a shared database.

The architecture will evolve toward independent services only when:

- Domain boundaries are stable.
- Independent deployment provides clear value.
- Independent scaling is required.
- Operational complexity is justified by business needs.

## Consecuencias

### Positivas
- Faster development during the early stages of the project.
- Simpler debugging and testing.
- Lower operational complexity.
- Easier refactoring while domain boundaries are being validated.
- Strong module boundaries that facilitate future extraction into
  independent services.

### Negativas / trade-offs asumidos
- Modules cannot be deployed independently.
- The entire application must be released together.
- Scaling remains tied to the whole system.
- Additional discipline is required to prevent tight coupling
  between modules.

### Conditions for Re-evaluation
- Independent deployment required from early stages of the project.
- Multiple teams need to work independently without coordination bottlenecks caused by a shared deployment unit.
- Domain boundaries are already well understood and are unlikely to change as the system evolves.