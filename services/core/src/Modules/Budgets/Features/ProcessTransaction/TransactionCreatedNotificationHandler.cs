using FinTrack.Common.Contracts;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Transactions.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinTrack.Modules.Budgets.Features.ProcessTransaction;

public class TransactionCreatedNotificationHandler(
    ApplicationDbContext context,
    ILogger<TransactionCreatedNotificationHandler> logger) : INotificationHandler<TransactionCreatedEvent>
{
    public async Task Handle(TransactionCreatedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Type != TransactionType.Expense) return;

        var budget = await context.Budgets.FirstOrDefaultAsync(b =>
            b.UserId == notification.UserId &&
            b.Category == notification.Category &&
            b.Month == notification.TransactionDate.Month &&
            b.Year == notification.TransactionDate.Year, cancellationToken);

        if (budget == null) return;
        budget.AmountSpent += notification.Amount;

        if (!budget.AlertSent && budget.AmountSpent >= budget.AmountLimit * 0.8m)
        {
            budget.AlertSent = true;
            logger.LogWarning(
                "Budget alert for user {UserId}: {Category} at {Percentae:P0} of limit ({Spent}/{Limit})",
                notification.UserId,
                notification.Category,
                budget.AmountSpent / budget.AmountLimit,
                budget.AmountSpent,
                budget.AmountLimit);
        }
        
        await context.SaveChangesAsync(cancellationToken);
    }
}