using BuildingBlocks.Abstractions.Messaging;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Core.Messaging.Extensions;

public static partial class ApplicationBuilderExtensions
{
    public static IApplicationBuilder SubscribeMessage<TMessage>(this IApplicationBuilder app)
        where TMessage : class, IMessage
    {
        app.ApplicationServices.GetRequiredService<IBus>().Consume<TMessage>();

        return app;
    }

    public static IApplicationBuilder SubscribeAllMessage(this IApplicationBuilder app)
    {
        app.ApplicationServices.GetRequiredService<IBus>().ConsumeAll();

        return app;
    }

    public static IApplicationBuilder SubscribeAllMessageFromAssemblyOfType<T>(this IApplicationBuilder app)
    {
        app.ApplicationServices.GetRequiredService<IBus>().ConsumeAllFromAssemblyOf<T>();

        return app;
    }
}
