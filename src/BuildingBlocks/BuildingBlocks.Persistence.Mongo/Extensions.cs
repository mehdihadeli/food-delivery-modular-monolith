using BuildingBlocks.Abstractions.Persistence.Mongo;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Persistence.Mongo
{
    public static class Extensions
    {
        public static IServiceCollection AddMongoDbContext<TContext>(
            this IServiceCollection services,
            IConfiguration configuration,
            string optionSection = nameof(MongoOptions),
            Action<MongoOptions>? configurator = null)
            where TContext : MongoDbContext
        {
            return services.AddMongoDbContext<TContext, TContext>(configuration, optionSection, configurator);
        }

        public static IServiceCollection AddMongoDbContext<TContextService, TContextImplementation>(
            this IServiceCollection services,
            IConfiguration configuration,
            string optionSection = nameof(MongoOptions),
            Action<MongoOptions>? configurator = null)
            where TContextService : IMongoDbContext
            where TContextImplementation : MongoDbContext, TContextService
        {
            services.Configure<MongoOptions>(configuration.GetSection(optionSection));
            if (configurator is { })
            {
                services.Configure(optionSection, configurator);
            }
            else
            {
                services.AddOptions<MongoOptions>().Bind(configuration.GetSection(optionSection))
                    .ValidateDataAnnotations();
            }

            services.AddScoped(typeof(TContextService), typeof(TContextImplementation));
            services.AddScoped(typeof(TContextImplementation));

            services.AddScoped<IMongoDbContext>(sp => sp.GetRequiredService<TContextService>());

            services.AddTransient(typeof(IMongoRepository<,>), typeof(MongoRepository<,>));
            services.AddTransient(typeof(IMongoUnitOfWork<>), typeof(MongoUnitOfWork<>));

            return services;
        }
    }
}
