using BuildingBlocks.Persistence.EfCore.Postgres;
using ECommerce.Modules.Catalogs.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task ApplyDatabaseMigrations(this IApplicationBuilder app, ILogger logger)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue<bool>(
                $"{CatalogModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:UseInMemory"))
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var catalogDbContext = serviceScope.ServiceProvider.GetRequiredService<CatalogDbContext>();

            logger.LogInformation("Updating catalog database...");

            await catalogDbContext.Database.MigrateAsync();

            logger.LogInformation("Updated catalog database");
        }
    }
}
