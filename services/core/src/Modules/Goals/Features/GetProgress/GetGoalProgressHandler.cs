using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Goals.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Goals.Features.GetProgress;

public class GetGoalProgressHandler(ApplicationDbContext context)
    : IRequestHandler<GetGoalProgressQuery, ErrorOr<GoalProgressResult>>
{
    public async Task<ErrorOr<GoalProgressResult>> Handle(
        GetGoalProgressQuery query, CancellationToken ct)
    {
        var goal = await context.Goals
            .AsNoTracking()
            .Where(g => g.Id == query.GoalId && g.UserId == query.UserId)
            .Select(g => new GoalProgressResult(
                g.Id,
                g.Name,
                g.Description,
                g.CurrentAmount,
                g.TargetAmount,
                g.TargetAmount > 0
                    ? Math.Round(g.CurrentAmount / g.TargetAmount * 100, 2)
                    : 0,
                g.Status.ToString(),
                g.TargetDate,
                g.CreatedAt,
                g.CompletedAt))
            .FirstOrDefaultAsync(ct);

        return goal is null
            ? GoalErrors.NotFound
            : goal;
    }
}