using Ardalis.GuardClauses;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Persistence.EfCore.Postgres;
using ECommerce.Modules.Catalogs;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Identity;
using ECommerce.Modules.Orders;

namespace ECommerce.Api.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddECommerceMonitoring(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMonitoring(healthChecksBuilder =>
        {
            var catalogPostgresOptions = configuration.GetOptions<PostgresOptions>(
                $"{CatalogModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}");

            Guard.Against.Null(catalogPostgresOptions, nameof(catalogPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                catalogPostgresOptions.ConnectionString,
                name: "Catalogs-Module-Postgres-Check",
                tags: new[] {"catalogs-postgres"});


            var customerPostgresOptions = configuration.GetOptions<PostgresOptions>(
                $"{CustomersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}");

            Guard.Against.Null(customerPostgresOptions, nameof(customerPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                customerPostgresOptions.ConnectionString,
                name: "Customers-Module-Postgres-Check",
                tags: new[] {"customers-postgres"});

            var identityPostgresOptions = configuration.GetOptions<PostgresOptions>(
                $"{IdentityModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}");
            Guard.Against.Null(identityPostgresOptions, nameof(identityPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                identityPostgresOptions.ConnectionString,
                name: "Identity-Module-Postgres-Check",
                tags: new[] {"identity-postgres"});

            var orderPostgresOptions =
                configuration.GetOptions<PostgresOptions>(
                    $"{OrdersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}");

            Guard.Against.Null(orderPostgresOptions, nameof(orderPostgresOptions));

            healthChecksBuilder.AddNpgSql(
                orderPostgresOptions.ConnectionString,
                name: "Orders-Modules-Postgres-Check",
                tags: new[] {"orders-postgres"});
        });

        return services;
    }
}
