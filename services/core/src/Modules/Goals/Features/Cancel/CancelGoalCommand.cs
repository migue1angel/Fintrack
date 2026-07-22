using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Goals.Features.Cancel;

public record CancelGoalCommand(Guid GoalId, Guid UserId)
    : IRequest<ErrorOr<CancelGoalResult>>;

public record CancelGoalResult(Guid GoalId, DateTime CancelledAt);