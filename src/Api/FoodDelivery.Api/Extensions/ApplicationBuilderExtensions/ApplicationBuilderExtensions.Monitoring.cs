using BuildingBlocks.Monitoring;

namespace FoodDelivery.Api.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseFoodDeliveryMonitoring(this IApplicationBuilder app)
    {
        app.UseMonitoring();

        return app;
    }
}
