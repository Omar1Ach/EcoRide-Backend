using EcoRide.BuildingBlocks.Application.Services;
using EcoRide.Modules.Security.Application.Data;
using EcoRide.Modules.Security.Application.Services;
using EcoRide.Modules.Security.Domain.Repositories;
using EcoRide.Modules.Security.Infrastructure.Persistence;
using EcoRide.Modules.Security.Infrastructure.Persistence.Repositories;
using EcoRide.Modules.Security.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcoRide.Modules.Security.Infrastructure;

/// <summary>
/// Extension methods for registering Security module infrastructure services
/// Follows Clean Architecture and Dependency Inversion Principle
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddSecurityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? dbContextOptions = null)
    {
        // Add DbContext
        if (dbContextOptions != null)
        {
            // Use custom database provider (for testing)
            services.AddDbContext<SecurityDbContext>(dbContextOptions);
        }
        else
        {
            // Use PostgreSQL for production
            services.AddDbContext<SecurityDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "security")));
        }

        // Register Security-specific Unit of Work (avoids conflicts with other modules)
        services.AddScoped<ISecurityUnitOfWork>(sp => sp.GetRequiredService<SecurityDbContext>());

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();

        // Register services
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // Use MockSmsService for development (prints OTP to console)
        // To use real SMS, change to: services.AddScoped<ISmsService, TwilioSmsService>();
        services.AddScoped<ISmsService, MockSmsService>();

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        // Register payment service (US-006: Credit card fallback and retry logic)
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}
