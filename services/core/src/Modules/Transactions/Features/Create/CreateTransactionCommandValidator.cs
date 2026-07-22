using FinTrack.Common.Contracts;
using FluentValidation;

namespace FinTrack.Modules.Transactions.Features.Create;

public class CreateTransactionCommandValidator: AbstractValidator<CreateTransactionCommnad>
{
    
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Type)
            .IsInEnum()
            .NotEqual(TransactionType.None)
            .WithMessage("Type must be 'Income', 'Expense'");

        RuleFor(x => x.Category)
            .NotEmpty().MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.TransactionDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow)) // consider adding 1 day to avoid issues with local utc and server time
            .WithMessage("Transactions cannot be in the future");
    }
}