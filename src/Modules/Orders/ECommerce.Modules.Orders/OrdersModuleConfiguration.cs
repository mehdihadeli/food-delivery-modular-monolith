using Asp.Versioning;
using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Extensions;
using ECommerce.Modules.Orders.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Orders.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Modules.Orders;

public class OrdersModuleConfiguration : IModuleDefinition
{
    public const string OrderModulePrefixUri = "api/v{version:apiVersion}/orders";
    public const string ModuleName = "Orders";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    public void AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddInfrastructure(configuration);

        services.AddStorage(configuration);

        // Add Sub Modules Services
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
            await app.ApplicationServices.StartHostedServices();
        }

        app.SubscribeAllMessageFromAssemblyOfType<OrdersRoot>();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
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
    }
}
