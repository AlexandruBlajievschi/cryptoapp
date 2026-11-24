using backend.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace backend.Test.Unit.Infrastructure.Persistence;

public class DatabaseConfigurationTests
{
    [Fact]
    public void ConnectionString_PreservesPasswordAndUsesConfiguredPort()
    {
        var configuration = Settings();
        configuration["POSTGRES_PASSWORD"] = "password;with=\"special characters";
        configuration["POSTGRES_PORT"] = "5433";

        var connection = new NpgsqlConnectionStringBuilder(
            DatabaseConfiguration.GetConnectionString(configuration));

        Assert.Equal("127.0.0.1", connection.Host);
        Assert.Equal(5433, connection.Port);
        Assert.Equal("learning", connection.Database);
        Assert.Equal("learner", connection.Username);
        Assert.Equal(configuration["POSTGRES_PASSWORD"], connection.Password);
    }

    [Fact]
    public void ExplicitConnectionString_TakesPrecedence()
    {
        var configuration = new ConfigurationManager();
        configuration["ConnectionStrings:DefaultConnection"] = "Host=database;Database=override";

        Assert.Equal("Host=database;Database=override",
            DatabaseConfiguration.GetConnectionString(configuration));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("65536")]
    [InlineData("invalid")]
    public void InvalidPort_IsRejected(string port)
    {
        var configuration = Settings();
        configuration["POSTGRES_PORT"] = port;

        Assert.Throws<InvalidOperationException>(() =>
            DatabaseConfiguration.GetConnectionString(configuration));
    }

    [Fact]
    public void MissingPassword_ReportsTheSettingWithoutExposingCredentials()
    {
        var configuration = Settings();
        configuration["POSTGRES_PASSWORD"] = "";

        var exception = Assert.Throws<InvalidOperationException>(() =>
            DatabaseConfiguration.GetConnectionString(configuration));

        Assert.Contains("POSTGRES_PASSWORD", exception.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EnvFile_LoadsFromRootOrBackend_AndPreservesOverrides(bool useBackendDirectory)
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var backend = Directory.CreateDirectory(Path.Combine(root, "backend")).FullName;
        try
        {
            File.WriteAllText(Path.Combine(root, ".env"),
                "# Local settings\nPOSTGRES_DB=learning\nPOSTGRES_USER=learner\n" +
                "POSTGRES_PASSWORD='local;password'\nPOSTGRES_PORT=5433\n");
            var configuration = new ConfigurationManager();
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["POSTGRES_PORT"] = "5434"
            });

            DatabaseConfiguration.LoadLocalEnv(configuration, useBackendDirectory ? backend : root);
            var connection = new NpgsqlConnectionStringBuilder(
                DatabaseConfiguration.GetConnectionString(configuration));

            Assert.Equal("learning", connection.Database);
            Assert.Equal("local;password", connection.Password);
            Assert.Equal(5434, connection.Port);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ConfigurationManager Settings()
    {
        var configuration = new ConfigurationManager();
        configuration["POSTGRES_DB"] = "learning";
        configuration["POSTGRES_USER"] = "learner";
        configuration["POSTGRES_PASSWORD"] = "local-password";
        return configuration;
    }
}
