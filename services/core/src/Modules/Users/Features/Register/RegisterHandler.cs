using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Users.Entities;
using FinTrack.Modules.Users.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Users.Features.Register;

public class RegisterHandler(ApplicationDbContext context): IRequestHandler<RegisterCommand, ErrorOr<RegisterResult>>
{
    public async Task<ErrorOr<RegisterResult>> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var normalizedEmail = command.Email.ToLowerInvariant();
        var emailExists = await context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
            return UserErrors.EmailAlreadyExists;
        
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password, workFactor: 12),
            FullName = command.FullName,
            CreatedAt = DateTime.UtcNow,
        };

        await context.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        
        return new RegisterResult(user.Id, user.Email);
    }
}