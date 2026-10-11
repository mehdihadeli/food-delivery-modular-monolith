using FoodDelivery.Api;
using FoodDelivery.Api.Extensions.HostApplicationBuilderExtensions;
using FoodDelivery.Api.Extensions.WebApplicationExtensions;
using FoodDelivery.ServiceDefaults.Extensions;
using Squirrel.Core.Web.Extensions;
using JasperFx;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.AddApplicationServices();

var app = builder.Build();

if (app.Environment.IsDependencyTest())
{
    return;
}

if (args.FirstOrDefault() is "db-apply" or "db-assert" or "db-dump" or "db-patch" or "db-list"
    or "storage" or "resources" or "help")
{
    Environment.ExitCode = await app.RunJasperFxCommands(args);
    return;
}

await app.UseInfrastructure();

app.MapApplicationEndpoints();
app.MapServiceDefaults();

Environment.ExitCode = await app.RunJasperFxCommands(args);
