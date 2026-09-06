# ADR-NNN: [Goals Module remains in Core]

**Status:**  Accepted 

## Context

The decision evaluates whether the Goals Module should be extracted as an independent microservice alongside budgets, or remain as port of the Core monolith.

There were two options considered:
- Extract Goals into its own microservice.
The module has clear technical boundaries and could be deployed independently. This would allow isolated scaling and ownership but would also introduce additional operational complexity.
- Keep Goals inside the Core application.
The module maintains clear internal boundaries while avoiding additional infrastructure, deployment pipelines, and inter-service communication.

## Decisión

Goals module will remain as part of the core project to avoid the complexity that comes with separating it as an independent microservice and because the scope of the project doesn't justify the overhead yet.

## Consequences

### Positive
- No extra infrastructure or deployment pipeline required
- Preserves clear module boundaries while allowing future extraction.

### Negative / accepted trade-offs
- Goals can only scale alongside the Core service
- If Goals eventually needs its own database, scaling or deployment pipeline, a migration will be required rather than a greenfield setup

### Conditions for Re-evaluation
- Goals need independent horizontal scaling