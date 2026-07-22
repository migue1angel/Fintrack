using ErrorOr;
using FinTrack.Common.Contracts;
using FinTrack.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Transactions.Features.GetHistory;

public class GetHistoryHandler(ApplicationDbContext context): IRequestHandler<GetTransactionHistoryQuery, ErrorOr<TransactionHistoryResult>>
{
    public async Task<ErrorOr<TransactionHistoryResult>> Handle(GetTransactionHistoryQuery query, CancellationToken cancellationToken)
    {
        if (query.Page < 1 || query.PageSize is < 1 or > 100)
            return PaginationErrors.InvalidPageSize(query.PageSize, 100);

        var queryable = context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == query.UserId);

        if (query.Category is not null)
            queryable = queryable.Where(t => t.Category == query.Category);
        
        if(query.Type != TransactionType.None)
            queryable = queryable.Where(t=> t.Type == query.Type);

        var totalCount = await queryable.CountAsync(cancellationToken);

        var items = await queryable
            .OrderByDescending(t => t.TransactionDate)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => new TransactionSummaryDto(
                t.Id, t.Amount, t.Type, t.Category, t.TransactionDate))
            .ToListAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);        
        return new TransactionHistoryResult(items, totalCount,query.Page, query.PageSize, totalPages);
    }
}