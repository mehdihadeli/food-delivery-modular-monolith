using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Web.Module;

namespace BuildingBlocks.Web;

public class GatewayProcessor<TModule> : IGatewayProcessor<TModule>
    where TModule : class, IModuleDefinition
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GatewayProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteCommand(Func<ICommandProcessor, IMapper, Task> action)
    {
        // https://blog.stephencleary.com/2016/12/eliding-async-await.html
        var compositionRoot = CompositionRootRegistry.GetByModule<TModule>();
        using var scope = compositionRoot?.CreateScope() ?? _scopeFactory.CreateScope();
        var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        await action.Invoke(commandProcessor, mapper);
    }

    public async Task ExecuteCommand(Func<ICommandProcessor, Task> action)
    {
        await ExecuteCommand(async (processor, _) => await action(processor));
    }

    public async Task<T> ExecuteCommand<T>(Func<ICommandProcessor, Task<T>> action)
    {
        return await ExecuteCommand(async (processor, _) => await action(processor)).ConfigureAwait(false);
    }

    public async Task<T> ExecuteCommand<T>(Func<ICommandProcessor, IMapper, Task<T>> action)
    {
        var compositionRoot = CompositionRootRegistry.GetByModule<TModule>();
        using var scope = compositionRoot?.CreateScope() ?? _scopeFactory.CreateScope();
        var commandProcessor = scope.ServiceProvider.GetRequiredService<ICommandProcessor>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        return await action.Invoke(commandProcessor, mapper);
    }

    public async Task<T> ExecuteQuery<T>(Func<IQueryProcessor, Task<T>> action)
    {
        return await ExecuteQuery(async (processor, _) => await action(processor)).ConfigureAwait(false);
    }

    public async Task<T> ExecuteQuery<T>(Func<IQueryProcessor, IMapper, Task<T>> action)
    {
        var compositionRoot = CompositionRootRegistry.GetByModule<TModule>();
        using var scope = compositionRoot?.CreateScope() ?? _scopeFactory.CreateScope();
        var queryProcessor = scope.ServiceProvider.GetRequiredService<IQueryProcessor>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        return await action.Invoke(queryProcessor, mapper);
    }
}
