using BuildingBlocks.Abstractions.Web.Module;

namespace BuildingBlocks.Web.Module;

public static class ModuleHook
{
    public static Action<IServiceCollection, IModuleDefinition>? ModuleServicesConfigured;
}
