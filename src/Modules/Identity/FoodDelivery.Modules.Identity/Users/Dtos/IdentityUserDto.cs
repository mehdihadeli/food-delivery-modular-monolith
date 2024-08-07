using FoodDelivery.Modules.Identity.Shared.Models;

namespace FoodDelivery.Modules.Identity.Users.Dtos;

public class IdentityUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime? LastLoggedInAt { get; set; }
    public IEnumerable<string>? RefreshTokens { get; set; }
    public IEnumerable<string>? Roles { get; set; }
    public UserState UserState { get; set; }
    public DateTime CreatedAt { get; set; }
}
