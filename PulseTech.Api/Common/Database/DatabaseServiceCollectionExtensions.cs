namespace PulseTech.Api.Common.Database;

public static class DatabaseServiceCollectionExtensions
{
    /// <summary>
    /// Registers database access using the <c>DefaultConnection</c> connection string.
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));

        return services;
    }
}
