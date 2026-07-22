using FluentValidation;

namespace FinTrack.Modules.Budgets.Features.Create;

public class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.Category)
            .NotEmpty().MaximumLength(100);

        RuleFor(x => x.AmountLimit)
            .GreaterThan(0).WithMessage("Amount limit must be greater than 0");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Month must be between 1 and 12");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Year must be between 2000 and 2100");
    }
}