using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Monitoring;
using ECommerce.Modules.Orders.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Orders.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Modules.Orders;

public class OrdersModuleConfiguration : IModuleDefinition
{
    public const string OrderModulePrefixUri = "api/v1/orders";

    public string ModuleRootName => TypeMapper.GetTypeName(GetType());

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
        app.UseMonitoring();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Orders Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();
    }
}
