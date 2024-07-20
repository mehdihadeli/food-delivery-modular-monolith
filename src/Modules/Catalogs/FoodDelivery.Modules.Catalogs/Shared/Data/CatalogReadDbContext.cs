using BuildingBlocks.Persistence.Mongo;
using FoodDelivery.Modules.Catalogs.Products.Models.Read;
using Humanizer;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FoodDelivery.Modules.Catalogs.Shared.Data;

public class CatalogReadDbContext : MongoDbContext
{
    public CatalogReadDbContext(IOptions<MongoOptions> options) : base(options)
    {
        Products = GetCollection<ProductReadModel>(nameof(Products).Underscore());
    }

    public IMongoCollection<ProductReadModel> Products { get; }
}
