using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;
using Hypothesist;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Shared.Fixtures;

public static class HandlerFactory
{
    public static MessageHandler<T> AsMessageHandler<T>(this IHypothesis<T> hypothesis) where T : class, IMessage
    {
        return (context, cancellationToken) => hypothesis.Test(context.Message, cancellationToken);
    }
}
