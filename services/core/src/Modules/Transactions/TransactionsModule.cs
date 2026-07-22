using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Modules.Transactions;

public sealed class TransactionsAssemblyMarker;

public static class TransactionsModule
{
    public static IServiceCollection AddTransactionsModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<TransactionsAssemblyMarker>());
        services.AddValidatorsFromAssemblyContaining<TransactionsAssemblyMarker>();

        return services;
    }
}