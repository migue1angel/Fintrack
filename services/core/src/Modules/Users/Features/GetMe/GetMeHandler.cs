using ErrorOr;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Users.Errors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Modules.Users.Features.GetMe;

public class GetMeHandler(ApplicationDbContext context) : IRequestHandler<GetMeQuery, ErrorOr<GetMeResult>>
{
    public async Task<ErrorOr<GetMeResult>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.UserId, cancellationToken);

        if (user is null)
            return UserErrors.NotFound;

        return new GetMeResult(user.Id, user.Email, user.FullName, user.CreatedAt);
    }
}