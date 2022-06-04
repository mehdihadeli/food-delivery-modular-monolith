using BuildingBlocks.Persistence.Mongo;
using ECommerce.Modules.Customers.Customers.Models.Reads;
using ECommerce.Modules.Customers.RestockSubscriptions.Models.Read;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ECommerce.Modules.Customers.Shared.Data;

public class CustomersReadDbContext : MongoDbContext
{
    public CustomersReadDbContext(IOptions<MongoOptions> options) : base(options)
    {
        RestockSubscriptions = GetCollection<RestockSubscriptionReadModel>(nameof(RestockSubscriptions).Underscore());
        Customers = GetCollection<CustomerReadModel>(nameof(Customers).Underscore());
    }

    public IMongoCollection<RestockSubscriptionReadModel> RestockSubscriptions { get; }
    public IMongoCollection<CustomerReadModel> Customers { get; }
}
