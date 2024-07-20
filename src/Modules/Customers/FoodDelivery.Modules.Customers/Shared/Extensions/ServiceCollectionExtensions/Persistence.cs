using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Persistence.EfCore.Postgres;
using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Modules.Customers.Shared.Contracts;
using FoodDelivery.Modules.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        AddStorage(builder.Services, configuration);

        return builder;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services, IConfiguration configuration)
    {
        AddPostgresWriteStorage(services, configuration);
        AddMongoReadStorage(services, configuration);

        return services;
    }

    private static void AddPostgresWriteStorage(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>(
                $"{CustomersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:UseInMemory"))
        {
            services.AddDbContext<CustomersDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Modules.Customers"));

            services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<CustomersDbContext>()!);
        }
        else
        {
            services.AddPostgresDbContext<CustomersDbContext>(
                configuration,
                $"{CustomersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}");
        }

        services.AddScoped<ICustomersDbContext>(provider => provider.GetRequiredService<CustomersDbContext>());
    }

    private static void AddMongoReadStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDbContext<CustomersReadDbContext>(
            configuration,
            $"{CustomersModuleConfiguration.ModuleName}:{nameof(MongoOptions)}");
    }
}
