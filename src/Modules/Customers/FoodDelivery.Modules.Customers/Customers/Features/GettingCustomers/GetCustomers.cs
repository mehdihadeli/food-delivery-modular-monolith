using AutoMapper;
using AutoMapper.QueryableExtensions;
using Squirrel.Abstractions.Queries;
using Squirrel.Core.Paging;
using Squirrel.Core.Queries;
using Squirrel.Persistence.Mongo.Extensions;
using Squirrel.Validation.Extensions;
using FoodDelivery.Modules.Customers.Customers.Dtos;
using FoodDelivery.Modules.Customers.Customers.Models.Reads;
using FoodDelivery.Modules.Customers.Shared.Data;
using FluentValidation;
using Sieve.Services;

namespace FoodDelivery.Modules.Customers.Customers.Features.GettingCustomers;

public record GetCustomers : PageQuery<GetCustomersResponse>
{
    public static GetCustomers Of(PageRequest pageRequest)
    {
        var (pageNumber, pageSize, filters, sortOrder) = pageRequest;

        return new GetCustomersValidator().HandleValidation(
            new GetCustomers
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = filters,
                SortOrder = sortOrder,
            }
        );
    }
}

public class GetCustomersValidator : AbstractValidator<GetCustomers>
{
    public GetCustomersValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GetCustomersHandler(
    CustomersReadDbContext customersReadDbContext,
    IMapper mapper,
    ISieveProcessor sieveProcessor
) : IQueryHandler<GetCustomers, GetCustomersResponse>
{
    public async ValueTask<GetCustomersResponse> Handle(GetCustomers request, CancellationToken cancellationToken)
    {
        var customer = await customersReadDbContext.Customers
            .ApplyPagingAsync<CustomerReadModel, CustomerReadDto, string>(
                request,
                sieveProcessor,
                source => source.ProjectTo<CustomerReadDto>(mapper.ConfigurationProvider),
                customer => customer.City,
                cancellationToken: cancellationToken);

        return new GetCustomersResponse(customer);
    }
}
