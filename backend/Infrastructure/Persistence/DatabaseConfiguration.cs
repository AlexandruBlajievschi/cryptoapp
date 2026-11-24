using Microsoft.Extensions.Configuration.Memory;
using Npgsql;

namespace backend.Infrastructure.Persistence;

public static class DatabaseConfiguration
{
    public static void LoadLocalEnv(ConfigurationManager configuration, string contentRootPath)
    {
        // dotnet run/EF use backend as the content root; running from the repo uses its root.
        var envPath = Path.Combine(contentRootPath, ".env");
        if (!File.Exists(envPath))
        {
            envPath = Path.GetFullPath(Path.Combine(contentRootPath, "..", ".env"));
        }

        if (!File.Exists(envPath))
        {
            return;
        }

        // Keep the file local to configuration, without changing process environment variables.
        // Existing environment variables and command-line configuration take precedence.
        var values = DotNetEnv.Env.NoEnvVars().Load(envPath)
            .Where(pair => pair.Key.StartsWith("POSTGRES_", StringComparison.Ordinal))
            .Select(pair => new KeyValuePair<string, string?>(pair.Key, pair.Value));

        configuration.Sources.Insert(0, new MemoryConfigurationSource { InitialData = values });
    }

    public static string GetConnectionString(IConfiguration configuration)
    {
        var explicitConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(explicitConnection))
        {
            return explicitConnection;
        }

        string Required(string key) => !string.IsNullOrWhiteSpace(configuration[key])
            ? configuration[key]!
            : throw new InvalidOperationException(
                $"Missing {key}. Configure the root .env in Development or supply database environment variables.");

        var portValue = configuration["POSTGRES_PORT"] ?? "5432";
        if (!int.TryParse(portValue, out var port) || port is < 1 or > 65535)
        {
            throw new InvalidOperationException("POSTGRES_PORT must be between 1 and 65535.");
        }

        return new NpgsqlConnectionStringBuilder
        {
            Host = configuration["POSTGRES_HOST"] ?? "127.0.0.1",
            Port = port,
            Database = Required("POSTGRES_DB"),
            Username = Required("POSTGRES_USER"),
            Password = Required("POSTGRES_PASSWORD")
        }.ConnectionString;
    }
}
