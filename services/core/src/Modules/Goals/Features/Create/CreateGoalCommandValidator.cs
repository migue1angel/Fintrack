using FluentValidation;

namespace FinTrack.Modules.Goals.Features.Create;

public class CreateGoalCommandValidator : AbstractValidator<CreateGoalCommand>
{
    public CreateGoalCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Goal name is required")
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0).WithMessage("Target amount must be greater than 0");

        RuleFor(x => x.TargetDate)
            .GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Target date must be in the future")
            .When(x => x.TargetDate.HasValue);
    }
}