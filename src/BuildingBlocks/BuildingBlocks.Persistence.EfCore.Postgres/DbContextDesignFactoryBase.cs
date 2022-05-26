using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BuildingBlocks.Persistence.EfCore.Postgres;

public abstract class DbContextDesignFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly string _postgresOptionSection;

    protected DbContextDesignFactoryBase(string postgresOptionSection)
    {
        _postgresOptionSection = postgresOptionSection;
    }

    public TDbContext CreateDbContext(string[] args)
    {
        Console.WriteLine($"BaseDirectory: {AppContext.BaseDirectory}");
        Console.WriteLine($"Postgres Option Section: {_postgresOptionSection}");

        var configuration = ConfigurationHelper.GetConfiguration(AppContext.BaseDirectory);
        var options = configuration.GetOptions<PostgresOptions>(_postgresOptionSection);

        if (string.IsNullOrWhiteSpace(options?.ConnectionString))
        {
            throw new InvalidOperationException("Could not find a connection string.");
        }

        Console.WriteLine($"Connection String: {options.ConnectionString}");

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(
                options.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(GetType().Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                }
            ).UseSnakeCaseNamingConvention();

        Console.WriteLine(options.ConnectionString);
        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options);
    }
}
