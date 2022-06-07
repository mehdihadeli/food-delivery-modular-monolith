using Microsoft.Extensions.Hosting;

namespace BuildingBlocks.Core.Extensions;

public static class ServiceProviderExtensions
{
    public static void StartHostedServices(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<IHostedService> hostedServices = serviceProvider.GetServices<IHostedService>();

        Task.WhenAll(hostedServices.Select(s => s.StartAsync(cancellationToken)));
    }

    public static Task StopHostedServices(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<IHostedService> hostedServices = serviceProvider.GetServices<IHostedService>();

        return Task.WhenAll(hostedServices.Select(s => s.StopAsync(cancellationToken)));
    }
}
