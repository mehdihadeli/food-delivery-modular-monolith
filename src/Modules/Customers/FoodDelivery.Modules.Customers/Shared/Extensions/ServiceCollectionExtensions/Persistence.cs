using Squirrel.Abstractions.Persistence;
using Squirrel.Abstractions.Events;
using Squirrel.Persistence.EfCore.Postgres;
using Squirrel.Persistence.Mongo;
using Squirrel.Persistence.Mongo.Extensions;
using FoodDelivery.Modules.Customers.Shared.Contracts;
using FoodDelivery.Modules.Customers.Shared.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace FoodDelivery.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        AddPostgresWriteStorage(builder, configuration);
        AddMongoReadStorage(builder, configuration);

        return builder;
    }

    private static void AddPostgresWriteStorage(WebApplicationBuilder builder, IConfiguration configuration)
    {
        var services = builder.Services;
        if (configuration.GetValue<bool>(
                $"{CustomersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:UseInMemory"))
        {
            services.AddDbContext<CustomersDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Modules.Customers"));

            services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<CustomersDbContext>()!);
            services.AddScoped<IDomainEventContext>(provider => provider.GetRequiredService<CustomersDbContext>());
        }
        else
        {
            builder.Configuration["ConnectionStrings:CustomersWrite"] =
                configuration[$"{CustomersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:ConnectionString"];
            builder.AddPostgresDbContext<CustomersDbContext>("CustomersWrite");
        }

        services.AddScoped<ICustomersDbContext>(provider => provider.GetRequiredService<CustomersDbContext>());
    }

    private static void AddMongoReadStorage(WebApplicationBuilder builder, IConfiguration configuration)
    {
        var mongoSection = configuration.GetSection($"{CustomersModuleConfiguration.ModuleName}:{nameof(MongoOptions)}");
        var mongoUrl = new MongoUrlBuilder(mongoSection["ConnectionString"]);
        mongoUrl.DatabaseName ??= mongoSection["DatabaseName"];
        builder.Configuration["ConnectionStrings:CustomersRead"] = mongoUrl.ToString();
        builder.AddMongoDbContext<CustomersReadDbContext>("CustomersRead");
    }
}
