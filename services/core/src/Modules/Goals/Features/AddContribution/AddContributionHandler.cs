using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Goals.Entities;
using FinTrack.Modules.Goals.Errors;
using FinTrack.Modules.Goals.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Goals.Features.AddContribution;

public class AddContributionHandler(ApplicationDbContext context, IMediator mediator)
    : IRequestHandler<AddContributionCommand, ErrorOr<AddContributionResult>>
{
    public async Task<ErrorOr<AddContributionResult>> Handle(AddContributionCommand command,
        CancellationToken cancellationToken)
    {
        var goal = await context.Goals
            .FirstOrDefaultAsync(g =>
                g.Id == command.GoalId && g.UserId == command.UserId, cancellationToken);

        if (goal is null)
            return GoalErrors.NotFound;

        if (goal.Status == GoalStatus.Cancelled)
            return GoalErrors.AlreadyCancelled;
        
        if (goal.Status == GoalStatus.Completed)
            return GoalErrors.AlreadyCompleted;

        var previousAmount = goal.CurrentAmount;
        goal.CurrentAmount += command.Amount;

        var isCompleted = false;

        if (goal.CurrentAmount >= goal.TargetAmount)
        {
            goal.CurrentAmount = goal.TargetAmount;
            goal.Status = GoalStatus.Completed;
            goal.CompletedAt = DateTime.UtcNow;
            isCompleted = true;
        }

        await context.SaveChangesAsync(cancellationToken);

        //Events

        await mediator.Publish(new GoalProgressUpdatedEvent(
            goal.Id,
            command.UserId,
            previousAmount,
            goal.CurrentAmount,
            goal.TargetAmount), cancellationToken);

        if (isCompleted)
            await mediator.Publish(new GoalReachedEvent(
                goal.Id,
                command.UserId,
                goal.Name,
                goal.TargetAmount,
                DateTime.UtcNow), cancellationToken);

        var progressPercentage = goal.TargetAmount > 0
            ? Math.Round(goal.CurrentAmount / goal.TargetAmount * 100, 2)
            : 0;

        return new AddContributionResult(
            goal.Id,
            goal.CurrentAmount,
            goal.TargetAmount,
            progressPercentage,
            isCompleted);
    }
}