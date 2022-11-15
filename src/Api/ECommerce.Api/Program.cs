using System.Reflection;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Logging;
using BuildingBlocks.Monitoring;
using BuildingBlocks.Security;
using BuildingBlocks.Security.Jwt;
using BuildingBlocks.Swagger;
using BuildingBlocks.Web;
using BuildingBlocks.Web.Extensions;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Web.Module;
using ECommerce.Api;
using ECommerce.Api.Extensions.ApplicationBuilderExtensions;
using ECommerce.Api.Extensions.ServiceCollectionExtensions;
using ECommerce.Modules.Catalogs;
using ECommerce.Modules.Customers;
using ECommerce.Modules.Identity;
using ECommerce.Modules.Orders;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using Serilog.Events;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis
// https://benfoster.io/blog/mvc-to-minimal-apis-aspnet-6/
var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder);

var app = builder.Build();

await ConfigureApplication(app);

await app.RunAsync();

static void RegisterServices(WebApplicationBuilder builder)
{
    builder.Host.UseDefaultServiceProvider((env, c) =>
    {
        // Handling Captive Dependency Problem
        // https://ankitvijay.net/2020/03/17/net-core-and-di-beware-of-captive-dependency/
        // https://levelup.gitconnected.com/top-misconceptions-about-dependency-injection-in-asp-net-core-c6a7afd14eb4
        // https://blog.ploeh.dk/2014/06/02/captive-dependency/
        if (env.HostingEnvironment.IsDevelopment() || env.HostingEnvironment.IsEnvironment("test") ||
            env.HostingEnvironment.IsStaging())
        {
            c.ValidateScopes = true;
        }
    });

    builder.Configuration.AddModulesSettingsFile(
        builder.Environment.ContentRootPath,
        builder.Environment.EnvironmentName);

    // https://www.michaco.net/blog/EnvironmentVariablesAndConfigurationInASPNETCoreApps#environment-variables-and-configuration
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#non-prefixed-environment-variables
    builder.Configuration.AddEnvironmentVariables("ecommerce_env_");

    // https://github.com/tonerdo/dotnet-env
    DotNetEnv.Env.TraversePath().Load();

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddECommerceMonitoring(builder.Configuration);

    builder.Services.AddApplicationOptions(builder.Configuration);
    var loggingOptions = builder.Configuration.GetSection(nameof(LoggerOptions)).Get<LoggerOptions>();

    builder.Host.AddCustomSerilog(
        optionsBuilder =>
        {
            optionsBuilder.SetLevel(LogEventLevel.Information);
        },
        config =>
        {
            config.WriteTo.File(
                ECommerce.Api.Program.GetLogPath(builder.Environment, loggingOptions) ??
                "../logs/customers-service.log",
                outputTemplate: loggingOptions?.LogTemplate ??
                                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true);
        });

    /*----------------- Module Services Setup ------------------*/
    builder.AddModulesServices(builder.Environment, useCompositionRootForModules: true);

    // https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc/
    builder.Services.AddControllers(options =>
            options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer())))
        .AddNewtonsoftJson(options =>
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

    builder.Services.ReplaceTransient<IControllerActivator, CustomServiceBasedControllerActivator>();

    builder.Services.AddCustomProblemDetails();
    builder.Services.AddCompression();

    builder.Services.AddCustomVersioning();
    builder.AddCustomSwagger(new[]
    {
        typeof(CatalogRoot).Assembly, typeof(IdentityRoot).Assembly, typeof(OrdersRoot).Assembly,
        typeof(CustomersRoot).Assembly,
    });

    builder.Services.AddCustomJwtAuthentication(builder.Configuration);
    builder.Services.AddCustomAuthorization(
        rolePolicies: new List<RolePolicy>
        {
            new(ApiConstants.Role.Admin, new List<string> {ApiConstants.Role.Admin}),
            new(ApiConstants.Role.User, new List<string> {ApiConstants.Role.User})
        });
}

static async Task ConfigureApplication(WebApplication app)
{
    var environment = app.Environment;

    app.UseProblemDetails();

    app.UseSerilogRequestLogging();

    app.UseRouting();
    app.UseAppCors();

    app.UseAuthentication();
    app.UseAuthorization();

    /*----------------- Module Middleware Setup ------------------*/
    await app.ConfigureModules();

    app.MapControllers();

    /*----------------- Module Routes Setup ------------------*/
    app.MapModulesEndpoints();

    // automatic discover minimal endpoints
    app.MapEndpoints();

    app.MapGet("/", (HttpContext _) => "ECommerce Modular Monolith Api.").ExcludeFromDescription();

    if (environment.IsDevelopment() || environment.IsEnvironment("docker"))
    {
        // swagger middleware should register after register endpoints to discover all endpoints and its versions correctly
        app.UseCustomSwagger();
    }

    app.UseECommerceMonitoring();

    app.Lifetime.ApplicationStopping.Register(() =>
    {
        if (app.Environment.IsEnvironment("test") == false)
        {
            foreach (var compositionRoot in CompositionRootRegistry.CompositionRoots)
            {
                compositionRoot.ServiceProvider.StopHostedServices().GetAwaiter().GetResult();
            }
        }
    });

    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();
}

namespace ECommerce.Api
{
    public partial class Program
    {
        public static string? GetLogPath(IWebHostEnvironment env, LoggerOptions loggerOptions)
            => env.IsDevelopment() ? loggerOptions.DevelopmentLogPath : loggerOptions.ProductionLogPath;
    }
}
