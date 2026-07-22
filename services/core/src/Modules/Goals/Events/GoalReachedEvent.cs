using MediatR;

namespace FinTrack.Modules.Goals.Events;

public record GoalReachedEvent(
    Guid GoalId,
    Guid UserId,
    string GoalName,
    decimal TargetAmount,
    DateTime OccurredAt) : INotification;