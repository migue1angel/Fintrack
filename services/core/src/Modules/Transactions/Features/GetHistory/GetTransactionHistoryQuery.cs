using ErrorOr;
using FinTrack.Common.Contracts;
using MediatR;

namespace FinTrack.Modules.Transactions.Features.GetHistory;

public record GetTransactionHistoryQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20,
    string? Category = null,
    TransactionType? Type = null) : IRequest<ErrorOr<TransactionHistoryResult>>;

public record TransactionHistoryResult(
    IReadOnlyList<TransactionSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public record TransactionSummaryDto(Guid Id, decimal Amount, TransactionType Type, string Category, DateOnly TransactionDate);