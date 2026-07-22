using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Users.Features.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<ErrorOr<GetMeResult>>;

public record GetMeResult(Guid UserId, string Email, string FullName, DateTime CreatedAt);