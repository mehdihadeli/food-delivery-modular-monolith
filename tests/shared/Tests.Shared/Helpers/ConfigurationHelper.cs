using Microsoft.Extensions.Configuration;
using Squirrel.Core.Extensions;

namespace Tests.Shared.Helpers;

public static class ConfigurationHelper
{
    private static readonly IConfigurationRoot _configurationRoot;

    static ConfigurationHelper()
    {
        _configurationRoot = BuildConfiguration();
    }

    /// <summary>
    /// Binds the options type from the section named after the type itself (e.g. <c>PostgresContainerOptions</c>).
    /// Used by the container fixtures so their settings can come from appsettings or environment variables.
    /// </summary>
    public static TOptions BindOptions<TOptions>()
        where TOptions : new()
    {
        return _configurationRoot.BindOptions<TOptions>();
    }

    //https://stackoverflow.com/questions/39791634/read-appsettings-json-values-in-net-core-test-project-xunit-mstest/
    //https://www.thecodebuzz.com/read-appsettings-json-in-net-core-test-project-xunit-mstest/
    //https://weblog.west-wind.com/posts/2018/Feb/18/Accessing-Configuration-in-NET-Core-Test-Projects
    //https://bartwullems.blogspot.com/2019/03/net-coreunit-tests-configuration.html
    public static IConfigurationRoot BuildConfiguration(string? basePath = null)
    {
        basePath ??= Directory.GetCurrentDirectory();

        return new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddJsonFile("appsettings.tests.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
}
