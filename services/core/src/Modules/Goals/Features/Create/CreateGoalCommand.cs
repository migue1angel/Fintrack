using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Goals.Features.Create;

public record CreateGoalCommand(
    Guid UserId,
    string Name,
    string? Description,
    decimal TargetAmount,
    DateOnly? TargetDate) : IRequest<ErrorOr<CreateGoalResult>>;

