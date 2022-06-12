using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Messaging.Extensions;
using BuildingBlocks.Web.Extensions;
using ECommerce.Modules.Identity.Identity;
using ECommerce.Modules.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;
using ECommerce.Modules.Identity.Users;

namespace ECommerce.Modules.Identity;

public class IdentityModuleConfiguration : IModuleDefinition
{
    public const string IdentityModulePrefixUri = "api/v1/identity";
    public const string ModuleName = "Identity";

    public void AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddInfrastructure(configuration);

        // Add Sub Modules Endpoints
        services.AddIdentityServices(configuration, environment);
        services.AddUsersServices(configuration);
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
            app.UseIdentityServer();

            // TODO: Add Monitoring
        }

        app.SubscribeAllMessageFromAssemblyOfType<IdentityRoot>();

        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("identity", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Identity Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        // Add Sub Modules Endpoints
        endpoints.MapIdentityEndpoints();
        endpoints.MapUsersEndpoints();
    }
}
