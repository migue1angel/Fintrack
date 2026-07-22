using FluentValidation;

namespace FinTrack.Modules.Goals.Features.AddContribution;

public class AddContributionCommandValidator : AbstractValidator<AddContributionCommand>
{
    public AddContributionCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Contribution amount must be greater than 0");

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => x.Note is not null);
    }
}