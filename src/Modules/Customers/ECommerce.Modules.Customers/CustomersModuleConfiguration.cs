using Asp.Versioning;
using Asp.Versioning.Builder;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Extensions;
using ECommerce.Modules.Customers.Customers;
using ECommerce.Modules.Customers.RestockSubscriptions;
using ECommerce.Modules.Customers.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Modules.Customers;

public class CustomersModuleConfiguration : IModuleDefinition
{
    public const string CustomerModulePrefixUri = "api/v{version:apiVersion}/customers";
    public const string ModuleName = "Customers";

    public void AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddInfrastructure(configuration);

        services.AddStorage(configuration);

        // Add Sub Modules Services
        services.AddCustomersServices();
        services.AddRestockSubscriptionServices();
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

        ServiceActivator.Configure(app.ApplicationServices);

        app.SubscribeAllMessageFromAssemblyOfType<CustomersRoot>();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Add Sub Modules Endpoints
        endpoints.MapCustomersEndpoints();
        endpoints.MapRestockSubscriptionsEndpoints();

        endpoints.MapGet("customers", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Customers Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();
    }
}
