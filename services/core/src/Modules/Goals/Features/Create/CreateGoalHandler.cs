using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Goals.Entities;
using MediatR;

namespace FinTrack.Modules.Goals.Features.Create;

public class CreateGoalHandler(ApplicationDbContext context)
    : IRequestHandler<CreateGoalCommand, ErrorOr<CreateGoalResult>>
{
    public async Task<ErrorOr<CreateGoalResult>> Handle(CreateGoalCommand command, CancellationToken cancellationToken)
    {
        var goal = new GoalEntity
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            Name = command.Name,
            Description = command.Description,
            TargetAmount = command.TargetAmount,
            CurrentAmount = 0,
            Status = GoalStatus.Active,
            TargetDate = command.TargetDate,
            CreatedAt = DateTime.UtcNow
        };

        await context.Goals.AddAsync(goal, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return new CreateGoalResult(goal.Id, goal.Name, goal.TargetAmount);
    }
}