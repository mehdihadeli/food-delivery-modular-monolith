using Squirrel.Abstractions.Core.Paging;
using FoodDelivery.Modules.Customers.Customers.Dtos;

namespace FoodDelivery.Modules.Customers.Customers.Features.GettingCustomers;

public record GetCustomersResponse(IPageList<CustomerReadDto> Customers);
