using FoodDelivery.Api.Extensions.HostApplicationBuilderExtensions;
using FoodDelivery.Api.Extensions.WebApplicationExtensions;
using Squirrel.Web.Extensions;
using Squirrel.Web.Module;
using Squirrel.Web.Minimal.Extensions;
using FoodDelivery.Modules.Catalogs;
using FoodDelivery.Modules.Customers;
using FoodDelivery.Modules.Identity;
using FoodDelivery.Modules.Orders;
using Squirrel.Abstractions.Web;
using Squirrel.Web;
using FoodDelivery.Api.Messaging;
using Squirrel.Abstractions.Messages;
using Squirrel.Abstractions.Web.Module;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Wolverine.Runtime;

namespace FoodDelivery.Api;

public static class ApplicationConfigurations
{
    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddHeaderPropagation();
        builder.Services.AddScoped(typeof(IGatewayProcessor<>), typeof(GatewayProcessor<>));
        foreach (var module in new IModuleConfiguration[]
        {
            new CatalogModuleConfiguration(), new CustomersModuleConfiguration(),
            new IdentityModuleConfiguration(), new OrdersModuleConfiguration(),
        })
        {
            var moduleBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = builder.Environment.EnvironmentName,
                ContentRootPath = builder.Environment.ContentRootPath,
                ApplicationName = builder.Environment.ApplicationName,
            });
            moduleBuilder.Configuration.AddConfiguration(builder.Configuration);
            foreach (var descriptor in builder.Services)
            {
                if (descriptor.ServiceType != typeof(ICompositionRoot)
                    && descriptor.ServiceType != typeof(IModuleConfiguration))
                {
                    moduleBuilder.Services.Add(descriptor);
                }
            }

            module.AddModuleServices(moduleBuilder);
            builder.Services.AddSingleton<ICompositionRoot>(host =>
            {
                foreach (var configure in host.GetKeyedServices<Action<IServiceCollection>>("ModuleTestServices"))
                {
                    configure(moduleBuilder.Services);
                }

                moduleBuilder.Services.RemoveAll<IExternalEventBus>();
                moduleBuilder.Services.AddScoped(_ => new ModuleMessagingSession(host.GetRequiredService<IWolverineRuntime>()));
                switch (module)
                {
                    case CatalogModuleConfiguration:
                        moduleBuilder.Services.AddDbContext<FoodDelivery.Modules.Catalogs.Shared.Data.CatalogDbContext>(
                            (provider, options) => options.AddInterceptors(provider.GetRequiredService<ModuleMessagingSession>()));
                        break;
                    case CustomersModuleConfiguration:
                        moduleBuilder.Services.AddDbContext<FoodDelivery.Modules.Customers.Shared.Data.CustomersDbContext>(
                            (provider, options) => options.AddInterceptors(provider.GetRequiredService<ModuleMessagingSession>()));
                        break;
                    case IdentityModuleConfiguration:
                        moduleBuilder.Services.AddDbContext<FoodDelivery.Modules.Identity.Shared.Data.IdentityContext>(
                            (provider, options) => options.AddInterceptors(provider.GetRequiredService<ModuleMessagingSession>()));
                        break;
                    case OrdersModuleConfiguration:
                        moduleBuilder.Services.AddDbContext<FoodDelivery.Modules.Orders.Shared.Data.OrdersDbContext>(
                            (provider, options) => options.AddInterceptors(provider.GetRequiredService<ModuleMessagingSession>()));
                        break;
                }
                moduleBuilder.Services.AddScoped<ModuleMessagingDatabase>(provider => module switch
                {
                    CatalogModuleConfiguration => new(provider.GetRequiredService<FoodDelivery.Modules.Catalogs.Shared.Data.CatalogDbContext>()),
                    CustomersModuleConfiguration => new(provider.GetRequiredService<FoodDelivery.Modules.Customers.Shared.Data.CustomersDbContext>()),
                    IdentityModuleConfiguration => new(provider.GetRequiredService<FoodDelivery.Modules.Identity.Shared.Data.IdentityContext>()),
                    OrdersModuleConfiguration => new(provider.GetRequiredService<FoodDelivery.Modules.Orders.Shared.Data.OrdersDbContext>()),
                    _ => throw new InvalidOperationException("No messaging database registered for module."),
                });
                moduleBuilder.Services.AddScoped<IExternalEventBus, ModuleEventBus>();
                moduleBuilder.Services.RemoveAll<Squirrel.Abstractions.Messages.MessagePersistence.IMessagePersistenceService>();
                moduleBuilder.Services.AddScoped<Squirrel.Abstractions.Messages.MessagePersistence.IMessagePersistenceService, ModuleMessagePersistenceService>();
                var provider = moduleBuilder.Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
                var root = new OwnedCompositionRoot(provider, module);
                CompositionRootRegistry.Add(root);
                return root;
            });
            builder.Services.AddSingleton<IModuleConfiguration>(_ => module);
        }
        builder.AddApplicationInfrastructure();

        return builder;
    }

    private sealed class OwnedCompositionRoot(ServiceProvider provider, IModuleConfiguration module)
        : ICompositionRoot, IAsyncDisposable
    {
        public IServiceProvider ServiceProvider => provider;
        public IModuleConfiguration ModuleDefinition => module;
        public IServiceScope CreateScope() => provider.CreateScope();

        public async ValueTask DisposeAsync()
        {
            CompositionRootRegistry.Remove(this);
            await provider.DisposeAsync();
        }
    }

    public static IEndpointRouteBuilder MapApplicationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
        endpoints.MapModulesEndpoints();
        endpoints.MapMinimalEndpoints();

        return endpoints;
    }

    public static WebApplication MapApplicationEndpoints(this WebApplication app)
    {
        ((IEndpointRouteBuilder)app).MapApplicationEndpoints();
        app.MapInfrastructureEndpoints();

        return app;
    }
}
