using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Transactions.Entities;
using FinTrack.Modules.Transactions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Transactions.Features.Create;

public class CreateTransactionHandler(ApplicationDbContext context, IMediator mediator)
    : IRequestHandler<CreateTransactionCommnad, ErrorOr<CreateTransactionResult>>
{
    public async Task<ErrorOr<CreateTransactionResult>> Handle(CreateTransactionCommnad command,
        CancellationToken cancellationToken)
    {
        if (command.IdempotencyKey is not null)
        {
            var existing = await context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IdempotencyKey == command.IdempotencyKey, cancellationToken);

            if (existing is not null)
                return new CreateTransactionResult(existing.Id, WasDuplicate: true);
        }

        var transaction = new TransactionEntity
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Amount = command.Amount,
            Type = command.Type,
            Category = command.Category,
            Description = command.Description,
            TransactionDate = command.TransactionDate,
            IdempotencyKey = command.IdempotencyKey,
            CreatedAt = DateTime.UtcNow
        };


        await context.Transactions.AddAsync(transaction, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        await mediator.Publish(new TransactionCreatedEvent(transaction.Id, transaction.UserId, transaction.Amount,
            transaction.Type, transaction.Category, transaction.TransactionDate,
            DateTime.UtcNow), cancellationToken);

        return new CreateTransactionResult(transaction.Id, WasDuplicate: false);
    }
}