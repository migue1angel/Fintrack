using FinTrack.Common.Contracts;

namespace FinTrack.Modules.Transactions.Features.Create;

public record CreateTransactionBody(
    decimal Amount,
    TransactionType Type,
    string Category,
    string? Description,
    DateOnly TransactionDate);

public record CreateTransactionResult(
    Guid TransactionId,
    bool WasDuplicate
);