using ECommerce.Modules.Identity.Users.Features.UpdatingUserState.Events.Integration;
using ECommerce.Modules.Shared.Identity.Users.Events.Integration;
using Humanizer;
using MassTransit;
using RabbitMQ.Client;

namespace ECommerce.Modules.Identity.Users;

internal static class MassTransitExtensions
{
    internal static void AddUserPublishers(this IRabbitMqBusFactoryConfigurator cfg)
    {
        cfg.Message<UserRegistered>(e =>
            e.SetEntityName($"{nameof(UserRegistered).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<UserRegistered>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<UserRegistered>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });

        cfg.Message<UserStateUpdated>(e =>
            e.SetEntityName($"{nameof(UserStateUpdated).Underscore()}.input_exchange")); // name of the primary exchange
        cfg.Publish<UserStateUpdated>(e => e.ExchangeType = ExchangeType.Direct); // primary exchange type
        cfg.Send<UserStateUpdated>(e =>
        {
            // route by message type to binding fanout exchange (exchange to exchange binding)
            e.UseRoutingKeyFormatter(context =>
                context.Message.GetType().Name.Underscore());
        });
    }
}
