using BuildingBlocks.Persistence.EfCore.Postgres;
using ECommerce.Modules.Orders.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task ApplyDatabaseMigrations(this IApplicationBuilder app, ILogger logger)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();

        if (!configuration.GetValue<bool>(
                $"{OrdersModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:UseInMemory"))
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<OrdersDbContext>();

            logger.LogInformation("Updating catalog database...");

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Updated catalog database");
        }
    }
}
