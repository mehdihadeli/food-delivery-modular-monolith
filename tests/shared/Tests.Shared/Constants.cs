namespace Tests.Shared;

public static class Constants
{
    public const string LoginApi = "http://localhost:5000/api/v1/identity/login";

    public static class Users
    {
        public static class Admin
        {
            public const string UserName = "mehdi";
            public const string Password = "123456";
            public const string Role = "admin";
        }

        public static class User
        {
            public const string UserName = "mehdi2";
            public const string Password = "123456";
            public const string Role = "user";
        }
    }

    public class AuthConstants
    {
        public const string Scheme = "TestAuth";
    }

}
