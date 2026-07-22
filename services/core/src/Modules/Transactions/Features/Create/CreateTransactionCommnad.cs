using ErrorOr;
using FinTrack.Common.Contracts;
using MediatR;

namespace FinTrack.Modules.Transactions.Features.Create;

public record CreateTransactionCommnad(
    Guid UserId,
    decimal Amount,
    TransactionType Type,
    string Category,
    string? Description,
    DateOnly TransactionDate,
    string? IdempotencyKey) : IRequest<ErrorOr<CreateTransactionResult>>;

