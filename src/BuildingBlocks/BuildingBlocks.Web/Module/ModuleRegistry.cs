using BuildingBlocks.Abstractions.Web.Module;

namespace BuildingBlocks.Web.Module;

public static class ModuleRegistry
{
    private static readonly List<IModuleDefinition> _moduleDefinitions = new();

    public static IReadOnlyList<IModuleDefinition> ModuleDefinitions => _moduleDefinitions.AsReadOnly();

    public static void Add(IModuleDefinition moduleDefinition)
    {
        _moduleDefinitions.Add(moduleDefinition);
    }

    public static void Remove(IModuleDefinition moduleDefinition)
    {
        _moduleDefinitions.Add(moduleDefinition);
    }

    public static IModuleDefinition? GetByAssemblyName(string assemblyName)
    {
        return _moduleDefinitions.FirstOrDefault(x => x.GetType().Assembly.GetName().Name == assemblyName);
    }

    public static IModuleDefinition? Get(IModuleDefinition moduleDefinition)
    {
        return _moduleDefinitions.FirstOrDefault(x => x == moduleDefinition);
    }

    public static IModuleDefinition? Get<TModule>()
        where TModule : class, IModuleDefinition
    {
        return _moduleDefinitions.FirstOrDefault(x => x.GetType() == typeof(TModule));
    }
}
