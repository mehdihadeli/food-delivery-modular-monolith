using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Web;
using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Web.Module;
using MediatR;

namespace BuildingBlocks.Web;

public class GatewayProcessor<TModule> : IGatewayProcessor<TModule>
    where TModule : class, IModuleDefinition
{
    private readonly IServiceProvider _serviceProvider;

    public GatewayProcessor(IServiceProvider serviceProvider)
    {
        // https://blog.stephencleary.com/2016/12/eliding-async-await.html
        var compositionRoot = CompositionRootRegistry.GetByModule<TModule>();
        _serviceProvider = compositionRoot?.ServiceProvider ?? serviceProvider;
    }

    public async ValueTask ExecuteScopeAsync(Func<IServiceProvider, ValueTask> action)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        await action(scope.ServiceProvider);
    }

    public async ValueTask<T> ExecuteScopeAsync<T>(Func<IServiceProvider, ValueTask<T>> action)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public async Task<TResponse> SendCommandAsync<TResponse>(
        ICommand<TResponse> request,
        CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    public async Task SendCommandAsync<T>(T request, CancellationToken cancellationToken = default)
        where T : class, ICommand
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(request, cancellationToken);
        });
    }

    public async Task ExecuteCommand<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            await commandProcessor.SendAsync(command, cancellationToken);
        });
    }

    public async Task<TResult> ExecuteCommand<TCommand, TResult>(
        TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
        where TResult : notnull
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();

            return await commandProcessor.SendAsync(command, cancellationToken);
        });
    }

    public async Task ExecuteCommand(Func<ICommandProcessor, IMapper, Task> action)
    {
        await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();
            var mapper = sp.GetRequiredService<IMapper>();

            await action.Invoke(commandProcessor, mapper);
        });
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
        return await ExecuteScopeAsync(async sp =>
        {
            var commandProcessor = sp.GetRequiredService<ICommandProcessor>();
            var mapper = sp.GetRequiredService<IMapper>();

            return await action.Invoke(commandProcessor, mapper);
        });
    }

    public async Task Publish(Func<IBus, Task> action)
    {
        var bus = _serviceProvider.GetRequiredService<IBus>();
        await action(bus);
    }

    public async Task<TResponse> SendQueryAsync<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default)
        where TResponse : class
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryProcessor>();

            return await queryProcessor.SendAsync(query, cancellationToken);
        });
    }

    public async Task<TResult> ExecuteQuery<TQuery, TResult>(
        TQuery query,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
        where TResult : notnull
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryProcessor>();

            return await queryProcessor.SendAsync(query, cancellationToken);
        });
    }

    public async Task<T> ExecuteQuery<T>(Func<IQueryProcessor, Task<T>> action)
    {
        return await ExecuteQuery(async (processor, _) => await action(processor))
            .ConfigureAwait(false);
    }

    public async Task<T> ExecuteQuery<T>(Func<IQueryProcessor, IMapper, Task<T>> action)
    {
        return await ExecuteScopeAsync(async sp =>
        {
            var queryProcessor = sp.GetRequiredService<IQueryProcessor>();
            var mapper = sp.GetRequiredService<IMapper>();

            return await action.Invoke(queryProcessor, mapper);
        });
    }
}
