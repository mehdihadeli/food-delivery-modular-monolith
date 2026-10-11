using Ardalis.GuardClauses;
using Squirrel.Abstractions.Commands;
using Squirrel.Abstractions.Messages;
using Squirrel.Core.Exception;
using FoodDelivery.Modules.Identity.Shared.Exceptions;
using FoodDelivery.Modules.Identity.Shared.Models;
using FoodDelivery.Modules.Identity.Users.Features.UpdatingUserState.Events.Integration;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Modules.Identity.Users.Features.UpdatingUserState;

public record UpdateUserState(Guid UserId, UserState State) : ITxCommand;

internal class UpdateUserStateValidator : AbstractValidator<UpdateUserState>
{
    public UpdateUserStateValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(v => v.State)
            .NotEmpty();

        RuleFor(v => v.UserId)
            .NotEmpty();
    }
}

internal class UpdateUserStateHandler : ICommandHandler<UpdateUserState>
{
    private readonly IExternalEventBus _bus;
    private readonly ILogger<UpdateUserStateHandler> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpdateUserStateHandler(
        IExternalEventBus bus,
        UserManager<ApplicationUser> userManager,
        ILogger<UpdateUserStateHandler> logger)
    {
        _bus = bus;
        _logger = logger;
        _userManager = Guard.Against.Null(userManager, nameof(userManager));
    }

    public async ValueTask<Unit> Handle(UpdateUserState request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
            throw new UserNotFoundException(request.UserId);

        var previousState = user.UserState;
        if (previousState == request.State)
        {
            return Unit.Value;
        }

        if (await _userManager.IsInRoleAsync(user, Constants.Role.Admin))
        {
            throw new UserStateCannotBeChangedException(request.State, request.UserId);
        }

        user.UserState = request.State;

        await _userManager.UpdateAsync(user);

        var userStateUpdated = new UserStateUpdated(
            request.UserId,
            (UserState)(int)previousState,
            (UserState)(int)request.State);

        await _bus.PublishAsync(userStateUpdated, cancellationToken: cancellationToken);


        _logger.LogInformation(
            "Updated state for user with ID: '{UserId}', '{PreviousState}' -> '{UserState}'",
            user.Id,
            previousState,
            user.UserState);

        return Unit.Value;
    }
}
