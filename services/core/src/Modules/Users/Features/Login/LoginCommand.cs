using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Users.Features.Login;

public record LoginCommand(string Email, string Password): IRequest<ErrorOr<LoginResult>>;

public record LoginResult(string Token, Guid UserId, string Email);