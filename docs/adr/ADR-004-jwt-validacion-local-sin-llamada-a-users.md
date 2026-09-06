# ADR-NNN: [jwt-validacion-local-sin-llamada-a-users]

**Estado:** Aceptado 

## Contexto

Authentication is owned by the Users Module, but JWT validation is required by all current and future modules/services. 
The Question is : Should services validate JTWs locally or delegate validation to a centralized Users service?
Authentication and JWT validation are different responsibilities

Points to consider: 
- Coupling
- Availability
- Scalability
- Token revocation capabilities
- Latency
- 
## Decisión

JWT validation will be performed locally by each module/service through the shared Common.Auth component.

The Users module remains responsible for authentication and token issuance, while validation and authorization are performed locally.

Token revocation will be handled through short-lived access tokens.
## Consecuencias

### Positivas
- No runtime dependency on Users. Decoupled services.
- High service autonomy

### Negativas / trade-offs asumidos
- More complex immediate token revocation.
- Shared signing keys require secure distribution and rotation mechanisms.

### Conditions for Re-evaluation
- Immediate token revocation becomes a business or security requirement.
- Short-lived access tokens no longer provide an acceptable security trade-off.
