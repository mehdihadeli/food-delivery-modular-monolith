using BuildingBlocks.Abstractions.Web.Module;

namespace BuildingBlocks.Web.Module;

// https://freecontent.manning.com/dependency-injection-in-net-2nd-edition-understanding-the-composition-root/
// https://blog.ploeh.dk/2011/07/28/CompositionRoot/
// http://www.kamilgrzybek.com/design/modular-monolith-domain-centric-design/
public static class CompositionRootRegistry
{
    private static readonly List<ICompositionRoot> _compositionRoots = new();

    public static IReadOnlyList<ICompositionRoot> CompositionRoots => _compositionRoots.AsReadOnly();

    public static IServiceProvider RootServiceProvider { get; private set; } = null!;

    public static void SetRootServiceProvider(IServiceProvider serviceProvider)
    {
        RootServiceProvider = serviceProvider;
    }

    public static void Add(ICompositionRoot compositionRoot)
    {
        _compositionRoots.Add(compositionRoot);
    }

    public static void Remove(ICompositionRoot compositionRoot)
    {
        _compositionRoots.Add(compositionRoot);
    }

    public static ICompositionRoot? GetByModuleByAssemblyName(string assemblyName)
    {
        return _compositionRoots.FirstOrDefault(x =>
            x.ModuleDefinition.GetType().Assembly.GetName().Name == assemblyName);
    }

    public static ICompositionRoot? GetByModule(IModuleDefinition moduleDefinition)
    {
        return _compositionRoots.FirstOrDefault(x => x.ModuleDefinition == moduleDefinition);
    }

    public static ICompositionRoot? GetByModule<TModule>()
        where TModule : class, IModuleDefinition
    {
        return _compositionRoots.FirstOrDefault(x => x.ModuleDefinition.GetType() == typeof(TModule));
    }
}
