using System.Diagnostics;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Messaging.Context;

namespace BuildingBlocks.Core.Messaging.Context;

public record ConsumeContext<TMessage>(
    TMessage Message,
    IDictionary<string, object?> Headers,
    Guid MessageId,
    string MessageType,
    int PayloadSize,
    ulong Version,
    DateTime Created) : IConsumeContext<TMessage>
    where TMessage : class, IMessage
{
    public ActivityContext? ParentContext { get; set; }
    public ContextItems Items { get; } = new();
    object IConsumeContext.Message => Message;
}

public record ConsumeContext(
    object Message,
    IDictionary<string, object?> Headers,
    Guid MessageId,
    string MessageType,
    int PayloadSize,
    ulong Version,
    DateTime Created) :
    IConsumeContext
{
    public ActivityContext? ParentContext { get; set; }
    public ContextItems Items { get; } = new();
}

public static class ConsumeContextExtensions
{
    public static ConsumeContext<TMessage> ToTypedConsumeContext<TMessage>(
        this ConsumeContext context)
        where TMessage : class, IMessage
    {
        return new ConsumeContext<TMessage>(
            (context.Message as TMessage)!,
            context.Headers,
            context.MessageId,
            context.MessageType,
            context.PayloadSize,
            context.Version,
            context.Created);
    }
}
