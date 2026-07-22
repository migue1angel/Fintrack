using MediatR;

namespace FinTrack.Modules.Goals.Events;

public record GoalProgressUpdatedEvent(
    Guid GoalId,
    Guid UserId,
    decimal PreviousAmount,
    decimal CurrentAmount,
    decimal TargetAmount) : INotification;