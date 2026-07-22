using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Goals.Features.AddContribution;

public record AddContributionCommand(
    Guid GoalId,
    Guid UserId,
    decimal Amount,
    string? Note) : IRequest<ErrorOr<AddContributionResult>>;

