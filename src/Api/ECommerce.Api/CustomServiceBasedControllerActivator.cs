using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Web.Module;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace ECommerce.Api;

/// <summary>
/// This class will customize built-in <see cref="ServiceBasedControllerActivator"/> class.
/// Ref: https://andrewlock.net/controller-activation-and-dependency-injection-in-asp-net-core-mvc
/// </summary>
public class CustomServiceBasedControllerActivator : IControllerActivator
{
    private IServiceScope _serviceScope;

    public object Create(ControllerContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var controllerAssembly = context.ActionDescriptor.ControllerTypeInfo.Assembly;

        IModuleDefinition? selectedModuleToRoute = null;
        foreach (var moduleDefinition in ModuleRegistry.ModuleDefinitions)
        {
            if (moduleDefinition.GetType().Assembly.GetName().Name == controllerAssembly.GetName().Name)
            {
                selectedModuleToRoute = moduleDefinition;
                break;
            }
        }

        if (selectedModuleToRoute is null)
            throw new Exception("Appropriate module for routed controller not found!");

        var compositionRoot = CompositionRootRegistry.GetByModule(selectedModuleToRoute);

        if (compositionRoot is null)
        {
            throw new Exception(
                $"Appropriate composition root for module `{selectedModuleToRoute.GetType().Name}` not found!");
        }

        _serviceScope = compositionRoot.ServiceProvider.CreateScope();

        Type serviceType = context.ActionDescriptor.ControllerTypeInfo.AsType();

        return _serviceScope.ServiceProvider.GetRequiredService(serviceType);
    }

    public void Release(ControllerContext context, object controller)
    {
        _serviceScope.Dispose();
    }
}
