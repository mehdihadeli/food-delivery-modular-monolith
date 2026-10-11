using FoodDelivery.Modules.Identity.UnitTests.Common;
using FoodDelivery.Modules.Identity.Users.Features.RegisteringUser;

namespace FoodDelivery.Modules.Identity.UnitTests.Users.Features.RegisteringUser;

public class RegisterUserValidatorTests : IdentityModuleUnitTestBase
{
    [Fact]
    public void valid_registration_has_no_validation_errors()
    {
        var command = RegisterUserFaker.Generate() with
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            UserName = "ada.lovelace",
            Email = "ada@example.com"
        };

        var result = new RegisterUserValidator().Validate(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void missing_first_name_reports_required_field()
    {
        var command = RegisterUserFaker.Generate() with { FirstName = "" };

        var result = new RegisterUserValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Match<FluentValidation.Results.ValidationFailure>(failure =>
                failure.PropertyName == nameof(RegisterUser.FirstName) &&
                failure.ErrorMessage == "FirstName is required.");
    }

    [Fact]
    public void mismatched_confirmation_reports_password_error()
    {
        var command = RegisterUserFaker.Generate() with { ConfirmPassword = "different-password" };

        var result = new RegisterUserValidator().Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.Should().Match<FluentValidation.Results.ValidationFailure>(failure =>
                failure.PropertyName == nameof(RegisterUser.ConfirmPassword) &&
                failure.ErrorMessage == "The password and confirmation password do not match.");
    }
}