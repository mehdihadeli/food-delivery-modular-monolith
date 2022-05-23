namespace BuildingBlocks.Abstractions.Web.Module;

public interface ICompositionRoot
{
    IServiceProvider ServiceProvider { get; }
    IModuleDefinition ModuleDefinition { get; }
    IServiceScope CreateScope();
}
