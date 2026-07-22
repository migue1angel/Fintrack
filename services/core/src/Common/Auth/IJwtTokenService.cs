namespace FinTrack.Common.Auth;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email);
    Guid? ExtractUserId(string? authorizationHeader);

}
