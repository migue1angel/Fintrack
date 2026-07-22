using ErrorOr;

namespace FinTrack.Modules.Users.Errors;

public static class UserErrors
{
    public static readonly Error EmailAlreadyExists = Error.Conflict(
        code: "User.EmailAlreadyExists",
        description: "An account with this email already exists");

    public static readonly Error InvalidCredentials = Error.Unauthorized(
        code: "Auth.InvalidCredentials",
        description: "Invalid email or password");

    public static readonly Error NotFound = Error.NotFound(
        code: "User.NotFound",
        description: "User not found");
}