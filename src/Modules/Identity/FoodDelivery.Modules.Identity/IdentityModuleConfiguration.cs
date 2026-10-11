using Asp.Versioning;
using Asp.Versioning.Builder;
using Squirrel.Abstractions.Web.Module;
using Squirrel.Core.Extensions;
using Squirrel.Core.Messages.Extensions;
using Squirrel.Web.Module;
using FoodDelivery.Modules.Identity.Identity;
using FoodDelivery.Modules.Identity.Shared.Extensions.ApplicationBuilderExtensions;
using FoodDelivery.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;
using FoodDelivery.Modules.Identity.Users;

namespace FoodDelivery.Modules.Identity;

public class IdentityModuleConfiguration : IModuleConfiguration
{
    public const string IdentityModulePrefixUri = "api/v{version:apiVersion}/identity";
    public const string ModuleName = "Identity";
    public static ApiVersionSet VersionSet { get; private set; } = default!;

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        builder.AddInfrastructure(builder.Configuration);

        // Add Sub Modules Endpoints
        builder.AddIdentityServices(builder.Configuration, builder.Environment);
        services.AddUsersServices(builder.Configuration);
        return builder;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        var moduleProvider = CompositionRootRegistry.GetByModule<IdentityModuleConfiguration>()?.ServiceProvider;
        if (app.Environment.IsEnvironment("test") == false)
        {
            if (moduleProvider is not null && !ReferenceEquals(moduleProvider, app.Services))
            {
                foreach (var hostedService in moduleProvider.GetServices<IHostedService>())
                {
                    await hostedService.StartAsync(app.Lifetime.ApplicationStopping);
                }
            }
            app.UseIdentityServer();

            // TODO: Add Monitoring
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
        return endpoints;
    }
}
