using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ErrorOr;
using FinTrack.Common.Auth;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Users.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace FinTrack.Modules.Users.Features.Login;

public class LoginHandler(ApplicationDbContext context, IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginCommand, ErrorOr<LoginResult>>
{
    
    public async Task<ErrorOr<LoginResult>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.ToLowerInvariant();
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash))
            return UserErrors.InvalidCredentials;

        var token = jwtTokenService.GenerateToken(user.Id, user.Email);
        return new LoginResult(token, user.Id, user.Email);
    }
    
}