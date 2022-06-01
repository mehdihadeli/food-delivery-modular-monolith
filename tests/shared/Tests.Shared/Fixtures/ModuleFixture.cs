using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public class ModuleFixture<TModule>
    where TModule : class, IModuleDefinition
{
    public ModuleFixture(IServiceProvider serviceProvider, IGatewayProcessor<TModule> gatewayProcessor, string name)
    {
        ServiceProvider = serviceProvider;
        Name = name;
        GatewayProcessor = gatewayProcessor;
    }

    public IServiceProvider ServiceProvider { get; }
    public string Name { get; }

    public IBus Bus => ServiceProvider.GetRequiredService<IBus>();

    public IGatewayProcessor<TModule> GatewayProcessor { get; }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = ServiceProvider.CreateScope();
        await action(scope.ServiceProvider);
    }
}
