using BuildingBlocks.Core.CQRS.Query;
using ECommerce.Modules.Customers.Customers.Dtos;

namespace ECommerce.Modules.Customers.Customers.Features.GettingCustomers;

public record GetCustomersResponse(ListResultModel<CustomerReadDto> Customers);
