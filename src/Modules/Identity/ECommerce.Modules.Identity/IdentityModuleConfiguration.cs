using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Monitoring;
using ECommerce.Modules.Identity.Identity;
using ECommerce.Modules.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;
using ECommerce.Modules.Identity.Users;

namespace ECommerce.Modules.Identity;

public class IdentityModuleConfiguration : IModuleDefinition
{
    public const string IdentityModulePrefixUri = "api/v1/identity";

    public void AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddIdentityServices(configuration);
        services.AddUsersServices(configuration);
    }

    public async Task ConfigureModule(
        IApplicationBuilder app,
        IConfiguration configuration,
        ILogger logger,
        IWebHostEnvironment environment)
    {
        app.UseMonitoring();

        app.UseIdentityServer();

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

            return $"Identity Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        endpoints.MapIdentityEndpoints();
        endpoints.MapUsersEndpoints();
    }
}
