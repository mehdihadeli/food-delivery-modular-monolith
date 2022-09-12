namespace BuildingBlocks.Web.Plugin;

[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
public sealed class WebApplicationPluginAttribute : Attribute
{
    public WebApplicationPluginAttribute(Type pluginType)
    {
        if (!(pluginType.IsClass && !pluginType.IsAbstract && typeof(WebApplicationPlugin).IsAssignableFrom(pluginType)))
        {
            throw new NotSupportedException($"{pluginType} is not a supported {nameof(WebApplicationPlugin)}");
        }

        PluginType = pluginType;
    }

    public Type PluginType { get; }
}
