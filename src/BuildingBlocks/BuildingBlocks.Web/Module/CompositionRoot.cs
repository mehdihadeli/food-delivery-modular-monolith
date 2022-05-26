using BuildingBlocks.Abstractions.Web.Module;

namespace BuildingBlocks.Web.Module;

public class CompositionRoot : ICompositionRoot
{
    public CompositionRoot(IServiceProvider serviceProvider, IModuleDefinition module)
    {
        ServiceProvider = serviceProvider;
        ModuleDefinition = module;
    }

    public IServiceProvider ServiceProvider { get; }
    public IModuleDefinition ModuleDefinition { get; }

    public IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }
}
