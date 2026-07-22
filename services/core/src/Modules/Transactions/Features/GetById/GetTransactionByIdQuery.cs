using ErrorOr;
using FinTrack.Common.Contracts;
using MediatR;

namespace FinTrack.Modules.Transactions.Features.GetById;

public record GetTransactionByIdQuery(Guid TransactionId, Guid UserId) : IRequest<ErrorOr<TransactionDetailDto>>;

public record TransactionDetailDto(
    Guid Id,
    Decimal Amount,
    TransactionType Type,
    string Category,
    string? Description,
    DateOnly TransactionDate,
    DateTime CreatedAt);