using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BuildingBlocks.Web.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection CreatNewCollection(this IServiceCollection services)
    {
        ServiceCollection newServiceCollection = new ServiceCollection();
        foreach (var service in services)
        {
            newServiceCollection.Add(service);
        }

        return newServiceCollection;
    }
}
