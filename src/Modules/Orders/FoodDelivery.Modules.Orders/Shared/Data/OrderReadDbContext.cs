using Squirrel.Persistence.Mongo;
using FoodDelivery.Modules.Orders.Orders.Models.Reads;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FoodDelivery.Modules.Orders.Shared.Data;

public class OrderReadDbContext : MongoDbContext
{
    public OrderReadDbContext(IMongoClient mongoClient, IMongoDatabase mongoDatabase) : base(mongoClient, mongoDatabase)
    {
        Orders = GetCollection<OrderReadModel>(nameof(Orders).Underscore());
    }

    public IMongoCollection<OrderReadModel> Orders { get; }
}
