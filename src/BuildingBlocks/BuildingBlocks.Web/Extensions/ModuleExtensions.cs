using System.Reflection;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Web.Module;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Web.Extensions;

// https://freecontent.manning.com/dependency-injection-in-net-2nd-edition-understanding-the-composition-root/
// https://blog.ploeh.dk/2011/07/28/CompositionRoot/
public static class ModuleExtensions
{
    public static void AddModuleServices<TModule>(
        this WebApplicationBuilder webApplicationBuilder,
        IWebHostEnvironment webHostEnvironment,
        bool useNewComposition = false)
        where TModule : class, IModuleDefinition
    {
        AddModuleServices<TModule>(
            webApplicationBuilder.Services,
            webApplicationBuilder.Configuration,
            webHostEnvironment,
            useNewComposition);
    }

    public static void AddModulesServices(
        this WebApplicationBuilder webApplicationBuilder,
        IWebHostEnvironment webHostEnvironment,
        bool useCompositionRootForModules = false,
        params Assembly[] scanAssemblies)
    {
        AddModulesServices(
            webApplicationBuilder.Services,
            webApplicationBuilder.Configuration,
            webHostEnvironment,
            useCompositionRootForModules,
            scanAssemblies);
    }

    public static void AddModulesServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        bool useCompositionRootForModules = false,
        params Assembly[] scanAssemblies)
    {
        var assemblies = scanAssemblies.Any() ? scanAssemblies : AppDomain.CurrentDomain.GetAssemblies();

        var modules = assemblies.SelectMany(x => x.GetTypes()).Where(t =>
            t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface
            && t.GetConstructor(Type.EmptyTypes) != null
            && typeof(IModuleDefinition).IsAssignableFrom(t)).ToList();

        foreach (var module in modules)
        {
            AddModuleServices(services, configuration, webHostEnvironment, module, useCompositionRootForModules);
        }

        // For handling specific composition root for processing commands and queries based on module
        services.AddGatewayProcessor();
    }

    public static void AddModuleServices<TModule>(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        bool useCompositionRootForModules = false)
        where TModule : class, IModuleDefinition
    {
        AddModuleServices(services, configuration, webHostEnvironment, typeof(TModule), useCompositionRootForModules);

        // For handling specific composition root for processing commands and queries based on module
        services.AddGatewayProcessor();
    }

    public static void AddModuleServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        Type moduleType,
        bool useCompositionRootForModules = false)
    {
        AddModulesDependency(services, configuration, webHostEnvironment, moduleType, useCompositionRootForModules);
    }

    private static void AddModulesDependency(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        Type module,
        bool useCompositionRootForModules)
    {
        IServiceCollection newServiceCollection =
            useCompositionRootForModules ? services.CreatNewServiceCollection() : services;

        var instantiatedType = (IModuleDefinition)Activator.CreateInstance(module)!;
        instantiatedType.AddModuleServices(newServiceCollection, configuration, webHostEnvironment);

        ModuleHook.ModuleServicesConfigured?.Invoke(newServiceCollection, instantiatedType);

        ModuleRegistry.Add(instantiatedType);

        if (useCompositionRootForModules)
        {
            CompositionRootRegistry.Add(new CompositionRoot(
                newServiceCollection.BuildServiceProvider(),
                instantiatedType));
        }
    }

    public static async Task ConfigureModules(this WebApplication app)
    {
        var modules = ModuleRegistry.ModuleDefinitions;

        foreach (var module in modules)
        {
            await ConfigureModule(app, module);
        }
    }

    public static async Task ConfigureModule<TModule>(
        this WebApplication app)
        where TModule : class, IModuleDefinition
    {
        var module = ModuleRegistry.Get<TModule>();
        if (module is null)
        {
            return;
        }

        await ConfigureModule(app, module);
    }

    public static async Task ConfigureModule(this WebApplication app, IModuleDefinition module)
    {
        CompositionRootRegistry.SetRootServiceProvider(app.Services);

        var compositionRoot = CompositionRootRegistry.GetByModule(module);
        if (compositionRoot is { })
        {
            // For composition roots we have to execute them manually because hosted services only runs for root service provider
            await module.ConfigureModule(
                new ApplicationBuilder(compositionRoot.ServiceProvider),
                app.Configuration,
                app.Logger,
                app.Environment);
        }
        else
        {
            CompositionRootRegistry.Add(new CompositionRoot(app.Services, module));
            await module.ConfigureModule(app, app.Configuration, app.Logger, app.Environment);
        }
    }

    public static void MapModulesEndpoints(this IEndpointRouteBuilder builder)
    {
        var modules = ModuleRegistry.ModuleDefinitions;

        foreach (var module in modules)
        {
            MapModuleEndpoints(builder, module);
        }
    }

    public static void MapModuleEndpoints<TModule>(this IEndpointRouteBuilder builder)
        where TModule : class, IModuleDefinition
    {
        var module = ModuleRegistry.Get<TModule>();
        if (module is null)
        {
            return;
        }

        MapModuleEndpoints(builder, module);
    }

    public static void MapModuleEndpoints(this IEndpointRouteBuilder builder, IModuleDefinition module)
    {
        var compositionRoot = CompositionRootRegistry.GetByModule(module);
        if (compositionRoot is null)
        {
            compositionRoot = new CompositionRoot(builder.ServiceProvider, module);
            CompositionRootRegistry.Add(compositionRoot);
        }

        module.MapEndpoints(builder);
    }

    public static ICompositionRoot? GetCurrentCompositionRoot(this Assembly assembly)
    {
        return CompositionRootRegistry.GetByModuleByAssemblyName(assembly.GetName().Name);
    }

    public static ICompositionRoot? GetCurrentCompositionRoot(this object instance)
    {
        return CompositionRootRegistry.GetByModuleByAssemblyName(instance.GetType().Assembly.GetName().Name);
    }

    public static void AddModulesSettingsFile(
        this ConfigurationManager configurationManager,
        string root,
        string environment)
    {
        foreach (string file in Directory.GetFiles(root, "*.appsettings.json", SearchOption.AllDirectories))
        {
            configurationManager.AddJsonFile(file);
        }

        foreach (string file in Directory.GetFiles(
                     root,
                     $"*.appsettings.{environment}.json",
                     SearchOption.AllDirectories))
        {
            configurationManager.AddJsonFile(file, true, true);
        }
    }
}
