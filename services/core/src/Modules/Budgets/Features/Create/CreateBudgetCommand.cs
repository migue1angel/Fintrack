using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Budgets.Features.Create;

public record CreateBudgetCommand(
    Guid UserId,
    string Category,
    decimal AmountLimit,
    int Month,
    int Year) : IRequest<ErrorOr<CreateBudgetResult>>;

public record CreateBudgetResult(
    Guid BudgetId,
    string Category,
    decimal AmountLimit,
    int Month,
    int Year);