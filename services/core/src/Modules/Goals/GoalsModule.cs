using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Modules.Goals;

public sealed class GoalsAssemblyMarker;

public static class GoalsModule
{
    public static IServiceCollection AddGoalsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<GoalsAssemblyMarker>());
        services.AddValidatorsFromAssemblyContaining<GoalsAssemblyMarker>();
        return services;
    }
}