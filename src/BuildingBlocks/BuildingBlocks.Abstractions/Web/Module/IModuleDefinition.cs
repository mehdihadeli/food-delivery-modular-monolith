using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Abstractions.Web.Module;

public interface IModuleDefinition
{
    string ModuleRootName { get; }

    void AddModuleServices(IServiceCollection services, IConfiguration configuration);

    Task ConfigureModule(
        IApplicationBuilder app,
        IConfiguration configuration,
        ILogger logger,
        IWebHostEnvironment environment);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
