using FoodDelivery.Api.Extensions.ServiceCollectionExtensions;
using FoodDelivery.Modules.Catalogs;
using FoodDelivery.Modules.Customers;
using FoodDelivery.Modules.Identity;
using FoodDelivery.Modules.Orders;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using Serilog.Events;
using Squirrel.Core.Extensions;
using Squirrel.Abstractions.Messages;
using Squirrel.Core.Messages;
using Squirrel.Core.Extensions.ServiceCollectionExtensions;
using Squirrel.Integration.Wolverine.InMemory;
using Squirrel.OpenApi;
using Squirrel.Security;
using Squirrel.Security.Jwt;
using Squirrel.SerilogLogging;
using Squirrel.SerilogLogging.Extensions;
using Squirrel.Web;
using Squirrel.Web.Extensions;
using Squirrel.Web.ProblemDetail;
using Squirrel.OpenApi.Swashbuckle.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Squirrel.Web.Minimal.Extensions;
using Wolverine.Postgresql;
using Wolverine;
using Wolverine.ErrorHandling;
using JasperFx;
using Npgsql;

namespace FoodDelivery.Api.Extensions.HostApplicationBuilderExtensions;

public static partial class HostApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Host.UseDefaultServiceProvider((env, options) =>
        {
            if (env.HostingEnvironment.IsDevelopment() || env.HostingEnvironment.IsEnvironment("test") ||
                env.HostingEnvironment.IsStaging())
            {
                options.ValidateScopes = true;
            }
        });

        foreach (var moduleName in new[] { "catalogs", "customers", "identity", "orders" })
        {
            var moduleSettings = $"{moduleName}.appsettings.json";
            var environmentSettings = $"{moduleName}.appsettings.{builder.Environment.EnvironmentName}.json";
            builder.Configuration.AddJsonFile(
                    builder.Environment.ContentRootFileProvider.GetFileInfo(moduleSettings).Exists
                        ? moduleSettings : Path.Combine(AppContext.BaseDirectory, moduleSettings), optional: true)
                .AddJsonFile(
                    builder.Environment.ContentRootFileProvider.GetFileInfo(environmentSettings).Exists
                        ? environmentSettings : Path.Combine(AppContext.BaseDirectory, environmentSettings), optional: true);
        }
        builder.Configuration.AddEnvironmentVariables("food_delivery_env_");
        DotNetEnv.Env.TraversePath().Load();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddFoodDeliveryHealthChecks(builder.Configuration);

        var loggingOptions = builder.Configuration.GetSection(nameof(SerilogOptions)).Get<SerilogOptions>();
        builder.AddCustomSerilog(
            config => config.MinimumLevel.Information().WriteTo.File(
                loggingOptions?.LogPath ?? "../logs/food-delivery.log",
                outputTemplate: loggingOptions?.LogTemplate ??
                                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level} - {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true));

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

        builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, CustomServiceBasedControllerActivator>());
        builder.AddCustomProblemDetails(new[] { typeof(Program).Assembly });
        builder.AddCustomVersioning();
        builder.AddMinimalEndpoints(typeof(ApiRoot).Assembly);
        builder.AddSwaggerOpenApi(typeof(ApiRoot).Assembly);

        builder.AddCustomJwtAuthentication(builder.Configuration);
        builder.Services.AddCustomJwtAuthorization(
            rolePolicies: new List<RolePolicy>
            {
                new(ApiConstants.Role.Admin, new List<string> { ApiConstants.Role.Admin }),
                new(ApiConstants.Role.User, new List<string> { ApiConstants.Role.User })
            });

        return builder;
    }

    internal static WebApplicationBuilder AddApplicationInfrastructure(this WebApplicationBuilder builder)
    {
        builder.Services.AddHeaderPropagation();
        builder.Services.AddScoped<IMessageMetadataAccessor, MessageMetadataAccessor>();
        builder.Services.AddOptions<MessagingOptions>().Bind(builder.Configuration.GetSection(nameof(MessagingOptions)));
        builder.Services.AddTransient(typeof(FoodDelivery.Api.Messaging.ModuleMessageHandler<,>));
        builder.Services.AddTransient<IExternalEventBus, WolverineInMemoryEventBus>();
        builder.Services.AddScoped<Squirrel.Abstractions.Messages.MessagePersistence.IMessagePersistenceService,
            WolverineInMemoryMessagePersistenceService>();
        builder.Services.AddWolverine(options =>
            {
                var connectionString = builder.Configuration["WolverineInMemoryBusOptions:DurabilityConnectionString"]
                    ?? builder.Configuration.GetConnectionString("wolverine")
                    ?? builder.Configuration["Identity:PostgresOptions:ConnectionString"]
                    ?? throw new InvalidOperationException("PostgreSQL messaging connection string is required.");
                var messagingDatabase = new NpgsqlConnectionStringBuilder(connectionString);
                foreach (var module in new[] { "Catalogs", "Customers", "Identity", "Orders" })
                {
                    var moduleConnection = builder.Configuration[$"{module}:PostgresOptions:ConnectionString"]
                        ?? throw new InvalidOperationException($"{module} PostgreSQL connection string is required.");
                    var moduleDatabase = new NpgsqlConnectionStringBuilder(moduleConnection);
                    if (!string.Equals(moduleDatabase.Host, messagingDatabase.Host, StringComparison.OrdinalIgnoreCase)
                        || moduleDatabase.Port != messagingDatabase.Port
                        || moduleDatabase.Database != messagingDatabase.Database)
                    {
                        throw new InvalidOperationException($"{module} and Wolverine must use the same PostgreSQL database for atomic messaging.");
                    }
                }
                options.UseRuntimeCompilation();
                options.PersistMessagesWithPostgresql(connectionString, "wolverine");
                options.Policies.UseDurableLocalQueues();
                options.Policies.OnException<Exception>().ScheduleRetry(
                    TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));
                options.Durability.KeepAfterMessageHandling = TimeSpan.FromDays(1);
                options.AutoBuildMessageStorageOnStartup = builder.Environment.IsProduction()
                    ? AutoCreate.None : AutoCreate.CreateOrUpdate;
                options.Discovery.DisableConventionalDiscovery();
                options.Discovery.IncludeType<FoodDelivery.Api.Messaging.IntegrationContractHandler>();
                foreach (var assembly in new[]
                {
                    typeof(CatalogRoot).Assembly, typeof(CustomersRoot).Assembly,
                    typeof(IdentityRoot).Assembly, typeof(OrdersRoot).Assembly,
                })
                {
                    foreach (var handler in assembly.GetTypes().Where(type => type is { IsClass: true, IsAbstract: false }))
                    {
                        foreach (var contract in handler.GetInterfaces().Where(type =>
                            type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Squirrel.Abstractions.Messages.IMessageHandler<>)))
                        {
                            options.Discovery.IncludeType(typeof(FoodDelivery.Api.Messaging.ModuleMessageHandler<,>)
                                .MakeGenericType(contract.GetGenericArguments()[0], handler));
                        }
                    }
                }
            });

        return builder;
    }

}
