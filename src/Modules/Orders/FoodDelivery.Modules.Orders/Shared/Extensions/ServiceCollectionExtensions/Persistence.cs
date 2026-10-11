using Squirrel.Abstractions.Persistence;
using Squirrel.Abstractions.Events;
using Squirrel.Persistence.EfCore.Postgres;
using Squirrel.Persistence.Mongo;
using Squirrel.Persistence.Mongo.Extensions;
using MongoDB.Driver;
using FoodDelivery.Modules.Orders.Shared.Contracts;
using FoodDelivery.Modules.Orders.Shared.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FoodDelivery.Modules.Orders.Shared.Extensions.ServiceCollectionExtensions;

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
                $"{OrdersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:UseInMemory"))
        {
            services.AddDbContext<OrdersDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Modules.Orders"));

            services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<OrdersDbContext>()!);
            services.AddScoped<IDomainEventContext>(provider => provider.GetRequiredService<OrdersDbContext>());
        }
        else
        {
            builder.Configuration["ConnectionStrings:OrdersWrite"] =
                configuration[$"{OrdersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:ConnectionString"];
            builder.AddPostgresDbContext<OrdersDbContext>("OrdersWrite");
        }

        services.AddScoped<IOrdersDbContext>(provider => provider.GetRequiredService<OrdersDbContext>());
    }

    private static void AddMongoReadStorage(WebApplicationBuilder builder, IConfiguration configuration)
    {
        var mongoSection = configuration.GetSection($"{OrdersModuleConfiguration.ModuleName}:{nameof(MongoOptions)}");
        var mongoUrl = new MongoUrlBuilder(mongoSection["ConnectionString"]);
        mongoUrl.DatabaseName ??= mongoSection["DatabaseName"];
        builder.Configuration["ConnectionStrings:OrdersRead"] = mongoUrl.ToString();
        builder.AddMongoDbContext<OrderReadDbContext>("OrdersRead");
    }
}
