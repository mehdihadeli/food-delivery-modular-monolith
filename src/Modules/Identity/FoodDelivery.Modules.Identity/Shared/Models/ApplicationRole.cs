using System.Globalization;
using Microsoft.AspNetCore.Identity;

namespace FoodDelivery.Modules.Identity.Shared.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = default!;

    public static ApplicationRole User => new()
    {
        Name = Constants.Role.User, NormalizedName = nameof(User).ToUpper(CultureInfo.InvariantCulture),
    };

    public static ApplicationRole Admin => new()
    {
        Name = Constants.Role.Admin, NormalizedName = nameof(Admin).ToUpper(CultureInfo.InvariantCulture)
    };
}
