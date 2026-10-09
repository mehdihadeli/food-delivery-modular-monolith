using Asp.Versioning;
using Asp.Versioning.Builder;
using Squirrel.Abstractions.Web.Module;
using Squirrel.Core.Extensions;
using Squirrel.Core.Messages.Extensions;
using FoodDelivery.Modules.Customers.Customers;
using FoodDelivery.Modules.Customers.RestockSubscriptions;
using FoodDelivery.Modules.Customers.Shared.Extensions.ApplicationBuilderExtensions;
using FoodDelivery.Modules.Customers.Shared.Extensions.ServiceCollectionExtensions;

namespace FoodDelivery.Modules.Customers;

public class CustomersModuleConfiguration : IModuleConfiguration
{
    public const string CustomerModulePrefixUri = "api/v{version:apiVersion}/customers";
    public const string ModuleName = "Customers";

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddInfrastructure(configuration);

        services.AddStorage(configuration);

        // Add Sub Modules Services
        services.AddCustomersServices();
        services.AddRestockSubscriptionServices();

        return builder;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        if (app.Environment.IsEnvironment("test") == false)
        {
            // HostedServices just run on main service provider - It should not await because it will block the main thread.
            await app.ApplicationServices.StartHostedServices();
        }

        ServiceActivator.Configure(app.ApplicationServices);

        app.SubscribeAllMessageFromAssemblyOfType<CustomersRoot>();

        await app.ApplyDatabaseMigrations(app.Logger);
        await app.SeedData(app.Logger, app.Environment);

        return app;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
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

        return endpoints;
    }
}
