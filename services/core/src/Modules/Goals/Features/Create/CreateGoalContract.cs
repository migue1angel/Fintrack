namespace FinTrack.Modules.Goals.Features.Create;

public record CreateGoalBody(
    string Name,
    string? Description,
    decimal TargetAmount,
    DateOnly? TargetDate);
    
public record CreateGoalResult(Guid GoalId, string Name, decimal TargetAmount);
    