using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Extensions;

// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services
public static class ServiceProviderExtensions
{
    public static Task StartHostedServices(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default,
        params Type[] hostedServiceTypes)
    {
        IEnumerable<IHostedService> hostedServices = serviceProvider.GetServices<IHostedService>();

        return Task.WhenAll(hostedServices.Select(s =>
        {
            if (hostedServiceTypes.Any() == false || hostedServiceTypes.Contains(s.GetType()))
                return s.StartAsync(cancellationToken);

            return Task.CompletedTask;
        }));
    }

    public static Task StopHostedServices(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default,
        params Type[] hostedServiceTypes)
    {
        IEnumerable<IHostedService> hostedServices = serviceProvider.GetServices<IHostedService>();

        return Task.WhenAll(hostedServices.Select(s =>
        {
            if (hostedServiceTypes.Any() == false || hostedServiceTypes.Contains(s.GetType()))
                return s.StopAsync(cancellationToken);

            return Task.CompletedTask;
        }));
    }
}
