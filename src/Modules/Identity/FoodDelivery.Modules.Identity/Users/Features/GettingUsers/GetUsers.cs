using AutoMapper;
using Squirrel.Abstractions.Queries;
using Squirrel.Core.Queries;
using Sieve.Services;
using Squirrel.Core.Persistence.EfCore;
using Squirrel.Core.Types;
using FoodDelivery.Modules.Identity.Shared.Extensions;
using FoodDelivery.Modules.Identity.Shared.Models;
using FoodDelivery.Modules.Identity.Users.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.Modules.Identity.Users.Features.GettingUsers;

public record GetUsers : PageQuery<GetUsersResponse>;

public class GetUsersValidator : AbstractValidator<GetUsers>
{
    public GetUsersValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
    }
}

public class GetUsersHandler : IQueryHandler<GetUsers, GetUsersResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly ISieveProcessor _sieveProcessor;

    public GetUsersHandler(UserManager<ApplicationUser> userManager, IMapper mapper, ISieveProcessor sieveProcessor)
    {
        _userManager = userManager;
        _mapper = mapper;
        _sieveProcessor = sieveProcessor;
    }

    public async ValueTask<GetUsersResponse> Handle(GetUsers request, CancellationToken cancellationToken)
    {
        var customer = await _userManager.FindAllUsersByPageAsync<IdentityUserDto>(_mapper, request, _sieveProcessor, cancellationToken);

        return new GetUsersResponse(customer);
    }
}
