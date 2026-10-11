using Squirrel.AspireIntegrations.Mongo;
using Squirrel.AspireIntegrations.Postgres;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres", userName: builder.AddParameter("postgres-user", "postgres"), password: builder.AddParameter("postgres-password", "postgres", secret: true));
var database = postgres.AddDatabase("food-delivery", "food_delivery");

var mongo = builder.AddMongoDB("mongo", userName: builder.AddParameter("mongo-user", "admin"), password: builder.AddParameter("mongo-password", "admin", secret: true));
var mongoDatabase = mongo.AddDatabase("food-delivery-mongo", "food_delivery");

var api = builder.AddProject<Projects.FoodDelivery_Api>("api")
    .WithReference(database)
    .WithReference(mongoDatabase)
    .WaitFor(database)
    .WaitFor(mongoDatabase)
    .WithEnvironment("Catalogs__PostgresOptions__ConnectionString", database)
    .WithEnvironment("Customers__PostgresOptions__ConnectionString", database)
    .WithEnvironment("Identity__PostgresOptions__ConnectionString", database)
    .WithEnvironment("Orders__PostgresOptions__ConnectionString", database)
    .WithEnvironment("Catalogs__MongoOptions__ConnectionString", mongoDatabase)
    .WithEnvironment("Customers__MongoOptions__ConnectionString", mongoDatabase)
    .WithEnvironment("Orders__MongoOptions__ConnectionString", mongoDatabase)
    .WithEnvironment("Catalogs__MongoOptions__DatabaseName", "food_delivery")
    .WithEnvironment("Customers__MongoOptions__DatabaseName", "food_delivery")
    .WithEnvironment("Orders__MongoOptions__DatabaseName", "food_delivery");

builder.Build().Run();
