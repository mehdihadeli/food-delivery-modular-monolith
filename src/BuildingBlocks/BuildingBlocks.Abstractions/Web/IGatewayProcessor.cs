using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Web.Module;

namespace BuildingBlocks.Abstractions.Web;

public interface IGatewayProcessor<TModule>
    where TModule : class, IModuleDefinition
{
    Task ExecuteCommand<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    Task<TResult> ExecuteCommand<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand<TResult>
        where TResult : notnull;

    Task ExecuteCommand(Func<ICommandProcessor, IMapper, Task> action);

    Task ExecuteCommand(Func<ICommandProcessor, Task> action);

    Task<T> ExecuteCommand<T>(Func<ICommandProcessor, Task<T>> action);

    Task<T> ExecuteCommand<T>(Func<ICommandProcessor, IMapper, Task<T>> action);

    Task Publish(Func<IBus, Task> action);

    Task<TResult> ExecuteQuery<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
        where TResult : notnull;

    Task<T> ExecuteQuery<T>(Func<IQueryProcessor, Task<T>> action);

    Task<T> ExecuteQuery<T>(
        Func<IQueryProcessor, IMapper, Task<T>> action);
}
