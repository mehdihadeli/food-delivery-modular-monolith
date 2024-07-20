using BuildingBlocks.Core.CQRS.Query;
using FoodDelivery.Modules.Customers.Customers.Dtos;

namespace FoodDelivery.Modules.Customers.Customers.Features.GettingCustomers;

public record GetCustomersResponse(ListResultModel<CustomerReadDto> Customers);
