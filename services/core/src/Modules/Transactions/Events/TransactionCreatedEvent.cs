using FinTrack.Common.Contracts;
using MediatR;

namespace FinTrack.Modules.Transactions.Events;


public record TransactionCreatedEvent (
    Guid TransactionId,
    Guid UserId,
    decimal Amount,
    TransactionType Type,
    string Category,
    DateOnly TransactionDate,
    DateTime OccurredAt):INotification;