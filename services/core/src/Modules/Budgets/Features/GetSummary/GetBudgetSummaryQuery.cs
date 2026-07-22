using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Budgets.Features.GetSummary;

public record GetBudgetSummaryQuery(
    Guid UserId,
    int Month,
    int Year) : IRequest<ErrorOr<BudgetSummaryResult>>;

public record BudgetSummaryResult(
    int Month,
    int Year,
    IReadOnlyList<BudgetSummaryItem> Items,
    decimal TotalBudgeted,
    decimal TotalSpent);

public record BudgetSummaryItem(
    Guid BudgetId,
    string Category,
    decimal AmountLimit,
    decimal AmountSpent,
    decimal ProgressPercentage,
    bool IsOverBudget,
    bool AlertSent);