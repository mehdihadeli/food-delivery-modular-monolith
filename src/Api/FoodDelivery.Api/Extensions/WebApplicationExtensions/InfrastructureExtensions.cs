using FoodDelivery.Api.Extensions.ApplicationBuilderExtensions;
using Serilog;
using Squirrel.Core.Extensions.ServiceCollectionExtensions;
using Squirrel.Web.Module;
using Squirrel.Web.Minimal.Extensions;
using Squirrel.OpenApi.Swashbuckle.Extensions;

namespace FoodDelivery.Api.Extensions.WebApplicationExtensions;

public static partial class WebApplicationExtensions
{
    public static async Task UseInfrastructure(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseSerilogRequestLogging();
        app.UseRouting();
        app.UseAppCors();
        app.UseAuthentication();
        app.UseAuthorization();

        _ = app.Services.GetServices<Squirrel.Abstractions.Web.Module.ICompositionRoot>().ToArray();
        await app.ConfigureModules();

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            if (app.Environment.IsEnvironment("test") == false)
            {
                foreach (var compositionRoot in CompositionRootRegistry.CompositionRoots)
                {
                    foreach (var hostedService in compositionRoot.ServiceProvider.GetServices<IHostedService>().Reverse())
                    {
                        hostedService.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
                    }
                }
            }
        });
    }

    internal static void MapInfrastructureEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("docker"))
        {
            app.UseSwaggerOpenApi();
        }
        else
        {
            app.MapGet("/", () => "Food Delivery Modular Monolith Api.").ExcludeFromDescription();
        }
    }
}
