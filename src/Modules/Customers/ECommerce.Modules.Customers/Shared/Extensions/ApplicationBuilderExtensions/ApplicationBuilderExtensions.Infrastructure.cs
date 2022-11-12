using BuildingBlocks.Monitoring;

namespace ECommerce.Modules.Catalogs.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseMonitoring();

        return app;
    }
}
