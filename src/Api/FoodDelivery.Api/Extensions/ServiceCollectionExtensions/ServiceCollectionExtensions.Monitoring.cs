using Ardalis.GuardClauses;
using Squirrel.Core.Extensions;
using Squirrel.Persistence.EfCore.Postgres;
using FoodDelivery.Modules.Catalogs;
using FoodDelivery.Modules.Customers;
using FoodDelivery.Modules.Identity;
using FoodDelivery.Modules.Orders;

namespace FoodDelivery.Api.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddFoodDeliveryHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();
            var catalogPostgresOptions = configuration.GetSection(
                $"{CatalogModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}").Get<PostgresOptions>();

            Guard.Against.Null(catalogPostgresOptions, nameof(catalogPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                catalogPostgresOptions.ConnectionString,
                name: "Catalogs-Module-Postgres-Check",
                tags: new[] {"catalogs-postgres"});


            var customerPostgresOptions = configuration.GetSection(
                $"{CustomersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}").Get<PostgresOptions>();

            Guard.Against.Null(customerPostgresOptions, nameof(customerPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                customerPostgresOptions.ConnectionString,
                name: "Customers-Module-Postgres-Check",
                tags: new[] {"customers-postgres"});

            var identityPostgresOptions = configuration.GetSection(
                $"{IdentityModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}").Get<PostgresOptions>();
            Guard.Against.Null(identityPostgresOptions, nameof(identityPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                identityPostgresOptions.ConnectionString,
                name: "Identity-Module-Postgres-Check",
                tags: new[] {"identity-postgres"});

            var orderPostgresOptions =
                configuration.GetSection(
                    $"{OrdersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}").Get<PostgresOptions>();

            Guard.Against.Null(orderPostgresOptions, nameof(orderPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                orderPostgresOptions.ConnectionString,
                name: "Orders-Modules-Postgres-Check",
                tags: new[] {"orders-postgres"});

        return services;
    }
}
