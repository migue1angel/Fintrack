using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Goals.Entities;
using FinTrack.Modules.Goals.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Goals.Features.Cancel;

public class CancelGoalHandler(ApplicationDbContext context)
    : IRequestHandler<CancelGoalCommand, ErrorOr<CancelGoalResult>>
{
    public async Task<ErrorOr<CancelGoalResult>> Handle(
        CancelGoalCommand command, CancellationToken ct)
    {
        var goal = await context.Goals
            .FirstOrDefaultAsync(g =>
                g.Id == command.GoalId && g.UserId == command.UserId, ct);

        if (goal is null)
            return GoalErrors.NotFound;
        
        if (goal.Status == GoalStatus.Completed)
            return GoalErrors.CannotCancelCompleted;

        if (goal.Status == GoalStatus.Cancelled)
            return GoalErrors.AlreadyCancelled;

        goal.Status = GoalStatus.Cancelled;
        goal.CancelledAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);

        return new CancelGoalResult(goal.Id, goal.CancelledAt!.Value);
    }
}