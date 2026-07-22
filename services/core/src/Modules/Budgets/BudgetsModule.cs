using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Modules.Budgets;

public sealed class BudgetsAssemblyMarker;

public static class BudgetsModule
{
    public static IServiceCollection AddBudgetsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<BudgetsAssemblyMarker>());
        services.AddValidatorsFromAssemblyContaining<BudgetsAssemblyMarker>();
        
        return services;
    }
}