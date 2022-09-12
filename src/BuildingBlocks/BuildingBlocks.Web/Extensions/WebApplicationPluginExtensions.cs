using System.Reflection;
using BuildingBlocks.Web.Plugin;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Web.Extensions;

// Ref: https://github.com/davidfowl/WebApplicationPlugins
public static class WebApplicationPluginExtensions
{
    public static void AddPlugins(this WebApplicationBuilder builder, string pluginSection = "Plugins")
    {
        var plugins = new List<PluginData>();

        var pluginsSection = builder.Configuration.GetSection(pluginSection);

        if (pluginsSection is null)
        {
            return;
        }

        var pluginConfig = new Dictionary<string, PluginConfig>();
        pluginsSection.Bind(pluginConfig);

        foreach (var (name, c) in pluginConfig)
        {
            var assemblyFile = Path.GetFullPath(c.AssemblyPath);
            var contentRootPath = c.ContentRootPath is not null ? Path.GetFullPath(c.ContentRootPath) : null;

            if (contentRootPath is not null)
            {
                // Add configuration from content root path
                var pluginAppSettingsFile = Path.Combine(contentRootPath, "appSettings.json");
                var pluginEnvAppSettingsFile = Path.Combine(contentRootPath, $"appSettings.{builder.Environment.EnvironmentName}.json");

                // Merge all of the configurations!
                builder.Configuration.AddJsonFile(pluginAppSettingsFile, optional: true, reloadOnChange: true);
                builder.Configuration.AddJsonFile(pluginEnvAppSettingsFile, optional: true, reloadOnChange: true);
            }

            var currentAssembly = Assembly.LoadFrom(assemblyFile);

            foreach (var attr in currentAssembly.GetCustomAttributes<WebApplicationPluginAttribute>())
            {
                var type = attr.PluginType;

                // Detect if those methods were overridden
                var doBuilder = type.GetMethod(nameof(WebApplicationPlugin.ConfigureWebApplicationBuilder))?.DeclaringType != typeof(WebApplicationPlugin);
                var doApp = type.GetMethod(nameof(WebApplicationPlugin.ConfigureWebApplication))?.DeclaringType != typeof(WebApplicationPlugin);

                // This type isn't instantiated using DI (chicken and egg problem)
                plugins.Add(new(doBuilder, doApp, (WebApplicationPlugin)Activator.CreateInstance(type)!));
            }
        }

        foreach (var p in plugins)
        {
            if (p.ConfigureWebApplicationBuilder)
            {
                p.Plugin.ConfigureWebApplicationBuilder(builder);

                // Use the same instance when mapping plugins
                builder.Services.AddSingleton(typeof(PluginData), p);
            }
        }
    }

    public static void MapPlugins(this WebApplication app)
    {
        foreach (var p in app.Services.GetServices<PluginData>())
        {
            if (p.ConfigureWebApplication)
            {
                p.Plugin.ConfigureWebApplication(app);
            }
        }
    }

    record PluginData(bool ConfigureWebApplicationBuilder,
                      bool ConfigureWebApplication,
                      WebApplicationPlugin Plugin);

    class PluginConfig
    {
        public string? ContentRootPath { get; set; }
        public string AssemblyPath { get; set; } = default!;
    }
}
