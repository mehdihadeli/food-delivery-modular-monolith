using Asp.Versioning;
using Asp.Versioning.Builder;
using Squirrel.Abstractions.Web.Module;
using Squirrel.Core.Extensions;
using Squirrel.Core.Messages.Extensions;
using Squirrel.Web.Module;
using FoodDelivery.Modules.Orders.Shared.Extensions.ApplicationBuilderExtensions;
using FoodDelivery.Modules.Orders.Shared.Extensions.ServiceCollectionExtensions;

namespace FoodDelivery.Modules.Orders;

public class OrdersModuleConfiguration : IModuleConfiguration
{
    public const string OrderModulePrefixUri = "api/v{version:apiVersion}/orders";
    public const string ModuleName = "Orders";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        builder.AddInfrastructure(builder.Configuration);
        builder.AddStorage(builder.Configuration);

        // Add Sub Modules Services
        return builder;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        var moduleProvider = CompositionRootRegistry.GetByModule<OrdersModuleConfiguration>()?.ServiceProvider;
        if (app.Environment.IsEnvironment("test") == false
            && moduleProvider is not null
            && !ReferenceEquals(moduleProvider, app.Services))
        {
            foreach (var hostedService in moduleProvider.GetServices<IHostedService>())
            {
                await hostedService.StartAsync(app.Lifetime.ApplicationStopping);
            }
        }
        var moduleApp = ((IApplicationBuilder)app).New();
        moduleApp.ApplicationServices = moduleProvider ?? app.Services;
        await moduleApp.ApplyDatabaseMigrations(app.Logger);
        await moduleApp.SeedData(app.Logger, app.Environment);
        return app;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var v1 = new ApiVersion(1, 0);
        var v2 = new ApiVersion(2, 0);
        var v3 = new ApiVersion(3, 0);

        VersionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(v1)
            .HasApiVersion(v2)
            .HasApiVersion(v3)
            .Build();

        endpoints.MapGet("orders", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Orders Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        // Add Sub Modules Endpoints
        return endpoints;
    }
}
