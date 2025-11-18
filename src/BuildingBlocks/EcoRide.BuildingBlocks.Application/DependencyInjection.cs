using EcoRide.BuildingBlocks.Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EcoRide.BuildingBlocks.Application;

/// <summary>
/// Extension methods for registering Application layer services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers MediatR, FluentValidation, and pipeline behaviors
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        params System.Reflection.Assembly[] assemblies)
    {
        // Register MediatR from the specified assemblies
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assemblies);

            // Register pipeline behaviors (order matters - they execute in registration order)
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });

        // Register all validators from the specified assemblies
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true);

        return services;
    }
}
