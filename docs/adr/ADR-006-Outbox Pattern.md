# ADR-NNN: [Outbox Pattern]

**Status:**  Accepted 

## Context
When a transaction is created, FinTrack must notify Budgets.
The current flow persists the transaction and then publishes TransactionCreatedEvent with MediatR:

SaveChangesAsync() commits the transaction.
mediator.Publish(...) notifies the handler.

If the commit succeeds but the application crashes or event publication fails, the transaction exists but Budgets is not updated. This risk increases when Budgets becomes an independent service and RabbitMQ is involved.

## Decisión
FinTrack will use the Transactional Outbox pattern for integration events from Transactions.
In one database transaction, the application saves:
The new transaction.
An OutboxEventEntity with the event type, serialized payload, and processing status.

A background OutboxPublisher reads pending events and publishes them to RabbitMQ through MassTransit. It marks an event as processed only after publication succeeds.

## Consequences

### Positive
- The transaction and its pending event are persisted atomically.
- RabbitMQ outages do not lose events; pending events can be retried.
- Transactions does not depend synchronously on Budgets.

### Negative / accepted trade-offs
- Budget updates are eventually consistent.
- The outbox requires monitoring and cleanup.
- Duplicate delivery is possible if publishing succeeds before the event is marked as processed; Budgets must be idempotent.

### Conditions for Re-evaluation
- We will re-evaluate this approach if event volume makes periodic database polling inefficient.