using Asp.Versioning;
using Asp.Versioning.Builder;
using Squirrel.Abstractions.Web.Module;
using Squirrel.Core.Extensions;
using Squirrel.Core.Messages.Extensions;
using Squirrel.Web.Module;
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

        builder.AddInfrastructure(configuration);

        builder.AddStorage(configuration);

        // Add Sub Modules Services
        services.AddCustomersServices();
        services.AddRestockSubscriptionServices();

        return builder;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        var moduleProvider = CompositionRootRegistry.GetByModule<CustomersModuleConfiguration>()?.ServiceProvider;
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
