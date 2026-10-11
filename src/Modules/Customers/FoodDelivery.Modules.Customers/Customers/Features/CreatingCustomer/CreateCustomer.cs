using Ardalis.GuardClauses;
using Squirrel.Abstractions.Commands;
using Squirrel.Core.Domain.ValueObjects;
using Squirrel.Core.Exception;
using Squirrel.Core.IdsGenerator;
using Squirrel.Validation.Extensions;
using FoodDelivery.Modules.Customers.Customers.Exceptions.Application;
using FoodDelivery.Modules.Customers.Customers.Models;
using FoodDelivery.Modules.Customers.Shared.Clients.Identity;
using FoodDelivery.Modules.Customers.Shared.Data;
using FluentValidation;
using Squirrel.Abstractions.Persistence;

namespace FoodDelivery.Modules.Customers.Customers.Features.CreatingCustomer;

public record CreateCustomer(string Email) : ICommand<CreateCustomerResponse>, ITxRequest
{
    public long Id { get; init; } = SnowFlakIdGenerator.NewId();

    public static CreateCustomer Of(string? email) =>
        new CreateCustomerValidator().HandleValidation(new CreateCustomer(email!));
}

internal class CreateCustomerValidator : AbstractValidator<CreateCustomer>
{
    public CreateCustomerValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is invalid.");
    }
}

public class CreateCustomerHandler(
    IIdentityApiClient identityApiClient,
    CustomersDbContext customersDbContext,
    ILogger<CreateCustomerHandler> logger
) : ICommandHandler<CreateCustomer, CreateCustomerResponse>
{
    public async ValueTask<CreateCustomerResponse> Handle(CreateCustomer command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var email = Email.Of(command.Email);
        if (customersDbContext.Customers.Any(x => x.Email == email))
            throw new CustomerAlreadyExistsException($"Customer with email '{command.Email}' already exists.");

        var identityUser = (await identityApiClient.GetUserByEmailAsync(command.Email, cancellationToken))
            ?.UserIdentity;

        if (identityUser is null)
            throw new CustomerNotFoundException($"Identity user with email '{command.Email}' not found.");

        var customer = Customer.Create(
            command.Id,
            Email.Of(identityUser.Email),
            CustomerName.Create(identityUser.FirstName, identityUser.LastName),
            identityUser.Id);

        await customersDbContext.AddAsync(customer, cancellationToken);

        await customersDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created a customer with ID: '{@CustomerId}'", customer.Id);

        return new CreateCustomerResponse(
            customer.Id,
            customer.Email!,
            customer.Name.FirstName,
            customer.Name.LastName,
            customer.IdentityId);
    }
}
