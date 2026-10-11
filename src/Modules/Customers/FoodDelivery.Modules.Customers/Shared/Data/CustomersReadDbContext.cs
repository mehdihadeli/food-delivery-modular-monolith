using Squirrel.Persistence.Mongo;
using FoodDelivery.Modules.Customers.Customers.Models.Reads;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Read;
using Humanizer;
using MongoDB.Driver;

namespace FoodDelivery.Modules.Customers.Shared.Data;

public class CustomersReadDbContext : MongoDbContext
{
    public CustomersReadDbContext(IMongoClient mongoClient, IMongoDatabase mongoDatabase)
        : base(mongoClient, mongoDatabase)
    {
        RestockSubscriptions = GetCollection<RestockSubscriptionReadModel>(nameof(RestockSubscriptions).Underscore());
        Customers = GetCollection<CustomerReadModel>(nameof(Customers).Underscore());
    }

    public IMongoCollection<RestockSubscriptionReadModel> RestockSubscriptions { get; }
    public IMongoCollection<CustomerReadModel> Customers { get; }
}
