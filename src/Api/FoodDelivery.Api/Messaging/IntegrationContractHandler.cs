using IdentityUserRegistered = FoodDelivery.Modules.Identity.Users.Features.RegisteringUser.Events.Integration.UserRegistered;
using CustomersUserRegistered = FoodDelivery.Modules.Customers.Users.Features.RegisteringUser.Events.External.UserRegistered;
using UserRegisteredConsumer = FoodDelivery.Modules.Customers.Users.Features.RegisteringUser.Events.External.UserRegisteredConsumer;
using CustomersCustomerCreated = FoodDelivery.Modules.Customers.Customers.Features.CreatingCustomer.Events.Integration.CustomerCreated;
using OrdersCustomerCreated = FoodDelivery.Modules.Orders.Customers.Features.CreatingCustomer.Events.External.CustomerCreated;
using CustomerCreatedConsumer = FoodDelivery.Modules.Orders.Customers.Features.CreatingCustomer.Events.External.CustomerCreatedConsumer;
using CatalogsProductCreated = FoodDelivery.Modules.Catalogs.Products.Features.CreatingProduct.Events.Integration.ProductCreated;
using CustomersProductCreated = FoodDelivery.Modules.Customers.Products.Features.CreatingProduct.Events.External.ProductCreated;
using ProductCreatedConsumer = FoodDelivery.Modules.Customers.Products.Features.CreatingProduct.Events.External.ProductCreatedConsumer;
using CatalogsProductStockReplenished = FoodDelivery.Modules.Catalogs.Products.Features.ReplenishingProductStock.Events.Integration.ProductStockReplenished;
using CustomersProductStockReplenished = FoodDelivery.Modules.Customers.Products.Features.ReplenishingProductStock.Events.External.ProductStockReplenished;
using ProductStockReplenishedConsumer = FoodDelivery.Modules.Customers.Products.Features.ReplenishingProductStock.Events.External.ProductStockReplenishedConsumer;

namespace FoodDelivery.Api.Messaging;

public sealed class IntegrationContractHandler
{
    public static Task Handle(IdentityUserRegistered message,
        ModuleMessageHandler<CustomersUserRegistered, UserRegisteredConsumer> handler,
        Wolverine.IMessageContext messaging,
        CancellationToken cancellationToken) =>
        handler.Handle(new CustomersUserRegistered(message.IdentityId, message.Email, message.UserName,
            message.FirstName, message.LastName, message.Roles), messaging, cancellationToken);

    public static Task Handle(CustomersCustomerCreated message,
        ModuleMessageHandler<OrdersCustomerCreated, CustomerCreatedConsumer> handler,
        Wolverine.IMessageContext messaging,
        CancellationToken cancellationToken) =>
        handler.Handle(new OrdersCustomerCreated(message.CustomerId), messaging, cancellationToken);

    public static Task Handle(CatalogsProductCreated message,
        ModuleMessageHandler<CustomersProductCreated, ProductCreatedConsumer> handler,
        Wolverine.IMessageContext messaging,
        CancellationToken cancellationToken) =>
        handler.Handle(new CustomersProductCreated(message.Id, message.Name, message.CategoryId,
            message.CategoryName, message.Stock), messaging, cancellationToken);

    public static Task Handle(CatalogsProductStockReplenished message,
        ModuleMessageHandler<CustomersProductStockReplenished, ProductStockReplenishedConsumer> handler,
        Wolverine.IMessageContext messaging,
        CancellationToken cancellationToken) =>
        handler.Handle(new CustomersProductStockReplenished(message.ProductId, message.NewStock,
            message.ReplenishedQuantity), messaging, cancellationToken);
}
