using BuildingBlocks.Abstractions.Web.Module;

namespace BuildingBlocks.Web.Module;

public static class CompositionRootRegistry
{
    private static readonly List<ICompositionRoot> _compositionRoots = new();

    public static IReadOnlyList<ICompositionRoot> CompositionRoots => _compositionRoots.AsReadOnly();

    public static void Add(ICompositionRoot compositionRoot)
    {
        _compositionRoots.Add(compositionRoot);
    }

    public static void Remove(ICompositionRoot compositionRoot)
    {
        _compositionRoots.Add(compositionRoot);
    }

    public static ICompositionRoot? GetByModuleName(string name)
    {
        return _compositionRoots.FirstOrDefault(x => x.ModuleDefinition.ModuleRootName == name);
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
