using AutoMapper;
using AutoMapper.QueryableExtensions;
using Squirrel.Abstractions.Queries;
using Squirrel.Core.Queries;
using Squirrel.Core.Types;
using Squirrel.Core.Extensions;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Dtos;
using FoodDelivery.Modules.Customers.RestockSubscriptions.Models.Read;
using FoodDelivery.Modules.Customers.Shared.Data;
using FluentValidation;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Sieve.Services;

namespace FoodDelivery.Modules.Customers.RestockSubscriptions.Features.GettingRestockSubscriptions;

public record GetRestockSubscriptions : PageQuery<GetRestockSubscriptionsResponse>
{
    public IList<string> Emails { get; init; } = new List<string>();
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

internal class GetRestockSubscriptionsValidator : AbstractValidator<GetRestockSubscriptions>
{
    public GetRestockSubscriptionsValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GeRestockSubscriptionsHandler : IQueryHandler<GetRestockSubscriptions, GetRestockSubscriptionsResponse>
{
    private readonly CustomersReadDbContext _customersReadDbContext;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;

    public GeRestockSubscriptionsHandler(CustomersReadDbContext customersReadDbContext, IMapper mapper, ISieveProcessor sieveProcessor)
    {
        _customersReadDbContext = customersReadDbContext;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
    }

    public async ValueTask<GetRestockSubscriptionsResponse> Handle(
        GetRestockSubscriptions query,
        CancellationToken cancellationToken)
    {
        var filtering = _customersReadDbContext.RestockSubscriptions.AsQueryable()
            .Where(x => x.IsDeleted == false)
            .Where(e => query.Emails.Any() == false || query.Emails.Contains(e.Email))
            .Where(x => (query.From == null && query.To == null) || (query.From == null && x.Created <= query.To) ||
                        (query.To == null && x.Created >= query.From) ||
                        (x.Created >= query.From && x.Created <= query.To))
            .OrderByDescending(x => x.Created);

        var restockSubscriptions =
            await filtering.ApplyPagingAsync<RestockSubscriptionReadModel, RestockSubscriptionDto, DateTime>(
                query,
                _sieveProcessor,
                source => source.ProjectTo<RestockSubscriptionDto>(_mapper.ConfigurationProvider),
                subscription => subscription.Created,
                cancellationToken: cancellationToken);

        return new GetRestockSubscriptionsResponse(restockSubscriptions);
    }
}
