using ErrorOr;
using MediatR;

namespace FinTrack.Modules.Users.Features.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName) : IRequest<ErrorOr<RegisterResult>>;

public record RegisterResult(Guid UserId, string Email);