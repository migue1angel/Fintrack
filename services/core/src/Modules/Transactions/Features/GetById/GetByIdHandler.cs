using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Transactions.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Transactions.Features.GetById;

public class GetByIdHandler(ApplicationDbContext context) : IRequestHandler<GetTransactionByIdQuery, ErrorOr<TransactionDetailDto>>
{
    public async Task<ErrorOr<TransactionDetailDto>> Handle(GetTransactionByIdQuery query, CancellationToken cancellationToken)
    {
        var transaction = await context.Transactions
            .AsNoTracking()
            .Where(t => t.Id == query.TransactionId && t.UserId == query.UserId)
            .Select(t => new TransactionDetailDto(
                t.Id,
                t.Amount,
                t.Type,
                t.Category,
                t.Description,
                t.TransactionDate,
                t.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        return transaction is null
            ? TransactionErrors.NotFound
            : transaction;
    }
}