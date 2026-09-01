using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Modules.Users;

public sealed class UsersAssemblyMarker;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<UsersAssemblyMarker>());
        services.AddValidatorsFromAssemblyContaining<UsersAssemblyMarker>();
        return services;
    }
}