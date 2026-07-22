namespace FinTrack.Modules.Goals.Features.AddContribution;

public record AddContributionBody(decimal Amount, string? Note);
public record AddContributionResult(
    Guid GoalId,
    decimal CurrentAmount,
    decimal TargetAmount,
    decimal ProgressPercentage,
    bool IsCompleted);