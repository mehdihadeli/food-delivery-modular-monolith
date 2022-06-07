using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Web.Extensions;
using ECommerce.Modules.Orders.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Orders.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Modules.Orders;

public class OrdersModuleConfiguration : IModuleDefinition
{
    public const string OrderModulePrefixUri = "api/v1/orders";
    public const string ModuleName = "Orders";

    public void AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddStorage(configuration);
    }

    public async Task ConfigureModule(
        IApplicationBuilder app,
        IConfiguration configuration,
        ILogger logger,
        IWebHostEnvironment environment)
    {
        if (environment.IsEnvironment("test") == false)
        {
            // HostedServices just run on main service provider - It should not await because it will block the main thread.
            app.ApplicationServices.StartHostedServices();
        }

        app.SubscribeAllMessageFromAssemblyOfType<OrdersRoot>();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("orders", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Orders Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();
    }
}
