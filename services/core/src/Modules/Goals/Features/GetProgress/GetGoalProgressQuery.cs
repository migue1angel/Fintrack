using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Goals.Features.GetProgress;

public record GetGoalProgressQuery(Guid GoalId, Guid UserId)
    : IRequest<ErrorOr<GoalProgressResult>>;

public record GoalProgressResult(
    Guid GoalId,
    string Name,
    string? Description,
    decimal CurrentAmount,
    decimal TargetAmount,
    decimal ProgressPercentage,
    string Status,
    DateOnly? TargetDate,
    DateTime CreatedAt,
    DateTime? CompletedAt);