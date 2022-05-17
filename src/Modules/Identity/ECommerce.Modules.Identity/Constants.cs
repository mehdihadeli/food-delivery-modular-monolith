namespace ECommerce.Modules.Identity;

public static class Constants
{
    public static class Role
    {
        public const string Admin = "admin";
        public const string User = "user";
    }

    public static string IdentityRoleName => "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
}
