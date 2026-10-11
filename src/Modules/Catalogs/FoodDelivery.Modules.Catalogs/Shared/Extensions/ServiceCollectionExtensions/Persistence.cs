using Squirrel.Abstractions.Persistence;
using Squirrel.Abstractions.Events;
using Squirrel.Persistence.EfCore.Postgres;
using Squirrel.Persistence.Mongo;
using Squirrel.Persistence.Mongo.Extensions;
using MongoDB.Driver;
using FoodDelivery.Modules.Catalogs.Shared.Contracts;
using FoodDelivery.Modules.Catalogs.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Catalogs.Shared.Extensions.ServiceCollectionExtensions;

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
                $"{CatalogModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:UseInMemory"))
        {
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Modules.Catalogs"));

            services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<CatalogDbContext>()!);
            services.AddScoped<IDomainEventContext>(provider => provider.GetRequiredService<CatalogDbContext>());
        }
        else
        {
            builder.Configuration["ConnectionStrings:CatalogsWrite"] =
                configuration[$"{CatalogModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:ConnectionString"];
            builder.AddPostgresDbContext<CatalogDbContext>("CatalogsWrite");
        }

        services.AddScoped<ICatalogDbContext>(provider => provider.GetRequiredService<CatalogDbContext>());
    }

    private static void AddMongoReadStorage(WebApplicationBuilder builder, IConfiguration configuration)
    {
        var mongoSection = configuration.GetSection($"{CatalogModuleConfiguration.ModuleName}:{nameof(MongoOptions)}");
        var mongoUrl = new MongoUrlBuilder(mongoSection["ConnectionString"]);
        mongoUrl.DatabaseName ??= mongoSection["DatabaseName"];
        builder.Configuration["ConnectionStrings:CatalogsRead"] = mongoUrl.ToString();
        builder.AddMongoDbContext<CatalogReadDbContext>("CatalogsRead");
    }
}
