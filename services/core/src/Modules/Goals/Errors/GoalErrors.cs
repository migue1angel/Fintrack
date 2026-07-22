using ErrorOr;

namespace FinTrack.Modules.Goals.Errors;

public static class GoalErrors
{
    public static Error NotFound = Error.NotFound(
        code: "Goal.NotFound",
        description: $"Goal  not found");

    public static readonly Error AlreadyCompleted = Error.Conflict(
        code: "Goal.AlreadyCompleted",
        description: "Cannot modify a completed goal");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        code: "Goal.AlreadyCancelled",
        description: "Cannot modify a cancelled goal");

    public static readonly Error NotActive = Error.Conflict(
        code: "Goal.NotActive",
        description: "Goal is not active");

    public static readonly Error InvalidContributionAmount = Error.Validation(
        code: "Goal.InvalidContributionAmount",
        description: "Contribution amount must be greater than 0");

    // Error específico para cuando el usuario intenta cancelar
    // una meta que ya alcanzó su objetivo.
    // Diferente de AlreadyCompleted porque la UI puede dar
    // un mensaje más preciso ("tu meta ya fue completada exitosamente").
    public static readonly Error CannotCancelCompleted = Error.Conflict(
        code: "Goal.CannotCancelCompleted",
        description: "Cannot cancel a goal that has already been completed");
}