using ErrorOr;

namespace FinTrack.Modules.Budgets.Errors;

public static class BudgetErrors
{
    public static readonly Error DuplicateBudget = Error.Conflict(
        code: "Budget.DuplicateBudget",
        description: "A budget for this category already exists for the selected month");

    public static Error NotFound(Guid budgetId) => Error.NotFound(
        code: "Budget.NotFound",
        description: $"Budget {budgetId} not found");

    public static readonly Error InvalidMonth = Error.Validation(
        code: "Budget.InvalidMonth",
        description: "Month must be between 1 and 12");

    public static readonly Error InvalidYear = Error.Validation(
        code: "Budget.InvalidYear",
        description: "Year must be between 2000 and 2100");
}