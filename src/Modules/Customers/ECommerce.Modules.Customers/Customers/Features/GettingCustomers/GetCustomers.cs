using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Core.CQRS.Query;
using BuildingBlocks.Core.Types;
using BuildingBlocks.Persistence.Mongo;
using ECommerce.Modules.Customers.Customers.Dtos;
using ECommerce.Modules.Customers.Customers.Models.Reads;
using ECommerce.Modules.Customers.Shared.Data;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ECommerce.Modules.Customers.Customers.Features.GettingCustomers;

public record GetCustomers : ListQuery<GetCustomersResponse>;

public class GetCustomersValidator : AbstractValidator<GetCustomers>
{
    public GetCustomersValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GetCustomersHandler : IQueryHandler<GetCustomers, GetCustomersResponse>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;

    public GetCustomersHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
    }

    public async Task<GetCustomersResponse> Handle(GetCustomers request, CancellationToken cancellationToken)
    {
        var customer = await _customersReadDbContext.Customers.AsQueryable()
            .OrderByDescending(x => x.City)
            .ApplyFilter(request.Filters)
            .ApplyPagingAsync<CustomerReadModel, CustomerReadDto>(
                _mapper.ConfigurationProvider,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);

        return new GetCustomersResponse(customer);
    }
}
