using Squirrel.Abstractions.Persistence;
using Squirrel.Persistence.EfCore.Postgres;
using FoodDelivery.Modules.Identity.Shared.Data;
using FoodDelivery.Modules.Identity.Shared.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FoodDelivery.Modules.Identity.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static WebApplicationBuilder AddCustomIdentity(
        this WebApplicationBuilder builder,
        IConfiguration configuration,
        string optionSection = nameof(IdentityOptions),
        Action<IdentityOptions>? configure = null)
    {
        var services = builder.Services;
        // Problem with .net core identity - will override our default authentication scheme `JwtBearerDefaults.AuthenticationScheme` to unwanted `FoodDelivery.Modules.Identity.Application` in `AddIdentity()` method .net identity
        // https://github.com/IdentityServer/IdentityServer4/issues/1525
        if (configuration.GetValue<bool>(
                $"{IdentityModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:UseInMemory"))
        {
            services.AddDbContext<IdentityContext>(options =>
                options.UseInMemoryDatabase("FoodDelivery.Modules.Identity"));

            services.AddScoped<IDbFacadeResolver>(provider => provider.GetService<IdentityContext>()!);
            services.AddScoped<IDomainEventContext>(provider => provider.GetRequiredService<IdentityContext>());
        }
        else
        {
            // Postgres
            builder.Configuration["ConnectionStrings:IdentityWrite"] =
                configuration[$"{IdentityModuleConfiguration.ModuleName}:{nameof(PostgresOptions)}:ConnectionString"];
            builder.AddPostgresDbContext<IdentityContext>("IdentityWrite");
        }

        // https://github.com/IdentityServer/IdentityServer4/issues/1525
        // some dependencies will add here if not registered before
        services.AddIdentity<ApplicationUser, ApplicationRole>(
                options =>
                {
                    // Password settings.
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredLength = 3;
                    options.Password.RequiredUniqueChars = 1;

                    // Lockout settings.
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings.
                    options.User.RequireUniqueEmail = true;

                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;

                    if (configure is { })
                        configure.Invoke(options);
                })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        if (configuration.GetSection(optionSection) is not null)
            services.Configure<IdentityOptions>(configuration.GetSection(optionSection));

        return builder;
    }
}
