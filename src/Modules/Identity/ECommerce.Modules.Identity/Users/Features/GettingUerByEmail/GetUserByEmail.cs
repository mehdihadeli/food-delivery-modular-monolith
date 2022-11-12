using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Query;
using ECommerce.Modules.Identity.Shared.Models;
using ECommerce.Modules.Identity.Users.Dtos;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Modules.Identity.Users.Features.GettingUerByEmail;

public record GetUserByEmail(string Email) : IQuery<GetUserByEmailResponse>;

internal class GetUserByIdValidator : AbstractValidator<GetUserByEmail>
{
    public GetUserByIdValidator()
    {
        CascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is not valid");
    }
}

internal class GetUserByEmailHandler : IQueryHandler<GetUserByEmail, GetUserByEmailResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetUserByEmailHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = Guard.Against.Null(userManager, nameof(userManager));
        _mapper = Guard.Against.Null(mapper, nameof(mapper));
    }

    public async Task<GetUserByEmailResponse> Handle(GetUserByEmail query, CancellationToken cancellationToken)
    {
        Guard.Against.Null(query, nameof(query));

        var identityUser = await _userManager.FindByEmailAsync(query.Email);

        var userDto = _mapper.Map<IdentityUserDto>(identityUser);

        return new GetUserByEmailResponse(userDto);
    }
}
