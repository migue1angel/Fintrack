using ErrorOr;
using FinTrack.Common.Contracts;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Budgets.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Budgets.Features.GetSummary;

public class GetBudgetSummaryHandler(ApplicationDbContext context)
    : IRequestHandler<GetBudgetSummaryQuery, ErrorOr<BudgetSummaryResult>>
{
    public async Task<ErrorOr<BudgetSummaryResult>> Handle(
        GetBudgetSummaryQuery query, CancellationToken ct)
    {
        if (query.Month is < 1 or > 12)
            return BudgetErrors.InvalidMonth;

        if (query.Year is < 2000 or > 2100)
            return BudgetErrors.InvalidYear;

        var budgets = await context.Budgets
            .AsNoTracking()
            .Where(b =>
                b.UserId == query.UserId &&
                b.Month == query.Month &&
                b.Year == query.Year)
            .ToListAsync(ct);

        var spentByCategory = await context.Transactions
            .AsNoTracking()
            .Where(t =>
                t.UserId == query.UserId &&
                t.Type == TransactionType.Expense &&
                t.TransactionDate.Month == query.Month &&
                t.TransactionDate.Year == query.Year)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(ct);

        var spentDict = spentByCategory.ToDictionary(x => x.Category, x => x.Total);

        var items = budgets.Select(b =>
        {
            var actualSpent = spentDict.GetValueOrDefault(b.Category, b.AmountSpent);
            var progress = b.AmountLimit > 0
                ? Math.Round(actualSpent / b.AmountLimit * 100, 2)
                : 0;

            return new BudgetSummaryItem(
                b.Id,
                b.Category,
                b.AmountLimit,
                actualSpent,
                progress,
                actualSpent > b.AmountLimit,
                b.AlertSent);
        }).ToList();

        return new BudgetSummaryResult(
            query.Month,
            query.Year,
            items,
            items.Sum(i => i.AmountLimit),
            items.Sum(i => i.AmountSpent));
    }
}