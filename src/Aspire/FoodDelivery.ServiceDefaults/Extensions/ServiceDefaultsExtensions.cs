using Squirrel.HealthCheck;
using Squirrel.OpenTelemetry.Extensions;
using Squirrel.Resiliency;
using Squirrel.Web.Extensions;

namespace FoodDelivery.ServiceDefaults.Extensions;

public static class ServiceDefaultsExtensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();
        builder.AddDefaultOpenTelemetry();
        builder.AddCustomResiliency();
        builder.AddCompression();

        return builder;
    }

    public static WebApplication MapServiceDefaults(this WebApplication app)
    {
        app.MapDefaultOpenTelemetry();
        app.MapDefaultHealthChecks();

        return app;
    }
}
