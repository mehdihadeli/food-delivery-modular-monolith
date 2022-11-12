using BuildingBlocks.Monitoring;

namespace ECommerce.Api.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseECommerceMonitoring(this IApplicationBuilder app)
    {
        app.UseMonitoring();

        return app;
    }
}
