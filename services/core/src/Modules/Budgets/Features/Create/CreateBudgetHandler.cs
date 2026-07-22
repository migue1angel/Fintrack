using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Budgets.Entities;
using FinTrack.Modules.Budgets.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Budgets.Features.Create;

public class CreateBudgetHandler(ApplicationDbContext context)
    : IRequestHandler<CreateBudgetCommand, ErrorOr<CreateBudgetResult>>
{
    public async Task<ErrorOr<CreateBudgetResult>> Handle(
        CreateBudgetCommand command, CancellationToken ct)
    {
        var exists = await context.Budgets.AnyAsync(b =>
            b.UserId == command.UserId &&
            b.Category == command.Category &&
            b.Month == command.Month &&
            b.Year == command.Year, ct);

        if (exists)
            return BudgetErrors.DuplicateBudget;

        var budget = new BudgetEntity
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Category = command.Category,
            AmountLimit = command.AmountLimit,
            AmountSpent = 0,
            AlertSent = false,
            Month = command.Month,
            Year = command.Year,
            CreatedAt = DateTime.UtcNow
        };

        await context.Budgets.AddAsync(budget, ct);
        await context.SaveChangesAsync(ct);

        return new CreateBudgetResult(
            budget.Id, budget.Category,
            budget.AmountLimit, budget.Month, budget.Year);
    }
}