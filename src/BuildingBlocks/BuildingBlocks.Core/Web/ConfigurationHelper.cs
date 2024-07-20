using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Core.Web;

public static class ConfigurationHelper
{
    public static IConfiguration GetConfiguration(string moduleName, string basePath)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile($"{moduleName}.appsettings.json")
            .AddJsonFile($"{moduleName}.appsettings.{environmentName}.json", true)
            .AddEnvironmentVariables();

        var config = builder.Build();

        return config;
    }
}
