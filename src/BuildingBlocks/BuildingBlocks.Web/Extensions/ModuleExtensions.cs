using System.Reflection;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;
using BuildingBlocks.Web.Module;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Web.Extensions;

// https://freecontent.manning.com/dependency-injection-in-net-2nd-edition-understanding-the-composition-root/
// https://blog.ploeh.dk/2011/07/28/CompositionRoot/
public static class ModuleExtensions
{
    public static void AddModuleServices<TModule>(
        this WebApplicationBuilder webApplicationBuilder,
        bool useNewComposition = false)
        where TModule : class, IModuleDefinition
    {
        AddModuleServices<TModule>(
            webApplicationBuilder.Services,
            webApplicationBuilder.Configuration,
            useNewComposition);
    }

    public static void AddModulesServices(
        this WebApplicationBuilder webApplicationBuilder,
        bool useNewCompositionRoot = false,
        params Assembly[] scanAssemblies)
    {
        AddModulesServices(
            webApplicationBuilder.Services,
            webApplicationBuilder.Configuration,
            useNewCompositionRoot,
            scanAssemblies);
    }

    public static void AddModulesServices(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useNewCompositionRoot = false,
        params Assembly[] scanAssemblies)
    {

        var assemblies = scanAssemblies.Any() ? scanAssemblies : AppDomain.CurrentDomain.GetAssemblies();

        var modules = assemblies.SelectMany(x => x.GetTypes()).Where(t =>
            t.IsClass && !t.IsAbstract && !t.IsGenericType && !t.IsInterface
            && t.GetConstructor(Type.EmptyTypes) != null
            && typeof(IModuleDefinition).IsAssignableFrom(t)).ToList();

        foreach (var module in modules)
        {
            AddModulesDependency(services, configuration, module, useNewCompositionRoot);
        }
    }

    public static void AddModuleServices<TModule>(
        this IServiceCollection services,
        IConfiguration configuration,
        bool useNewCompositionRoot = false)
        where TModule : class, IModuleDefinition
    {
        IServiceCollection newServiceCollection = useNewCompositionRoot ? services.CreatNewCollection() : services;

        AddModulesDependency(newServiceCollection, configuration, typeof(TModule), useNewCompositionRoot);
    }

    private static void AddModulesDependency(
        IServiceCollection services,
        IConfiguration configuration,
        Type module,
        bool useNewCompositionRoot)
    {
        IServiceCollection newServiceCollection = useNewCompositionRoot ? services.CreatNewCollection() : services;

        var instantiatedType = (IModuleDefinition)Activator.CreateInstance(module)!;
        instantiatedType.AddModuleServices(newServiceCollection, configuration);

        ModuleRegistry.Add(instantiatedType);

        if (useNewCompositionRoot)
        {
            CompositionRootRegistry.Add(new CompositionRoot(newServiceCollection.BuildServiceProvider(), instantiatedType));
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
        var compositionRoot = CompositionRootRegistry.GetByModule(module);
        if (compositionRoot is { })
        {
            await module.ConfigureModule(
                new ApplicationBuilder(compositionRoot.ServiceProvider),
                app.Configuration,
                app.Logger,
                app.Environment);
        }
        else
        {
            await module.ConfigureModule(app, app.Configuration, app.Logger, app.Environment);
        }
    }

    public static void MapModulesEndpoints(
        this IEndpointRouteBuilder builder,
        params Assembly[] scanAssemblies)
    {
        var modules = ModuleRegistry.ModuleDefinitions;

        foreach (var module in modules)
        {
            module.MapEndpoints(builder);
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

        module.MapEndpoints(builder);
    }

    public static void MapModuleEndpoints(this IEndpointRouteBuilder builder, IModuleDefinition module)
    {
        module.MapEndpoints(builder);
    }
}
