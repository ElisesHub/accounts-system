using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using DotNet.Testcontainers.Configurations;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace AccountsAPI.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestApiKey = "integration-test-api-key";

    private const string AppPassword = "integration-test-app-password";

    private static readonly string InitDirectory =
        Path.Combine(AppContext.BaseDirectory, "TestData", "init");

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18")
        .WithDatabase("accountsdata")
        .WithUsername("accounts_admin")
        .WithPassword("integration-test-admin-password")
        .WithResourceMapping(Encoding.UTF8.GetBytes(AppPassword), "/run/secrets/DbAppPassword")
        .WithResourceMapping(
            new FileInfo(Path.Combine(InitDirectory, "00_create_app_role.sh")),
            "/docker-entrypoint-initdb.d/",
            fileMode: Unix.FileMode755)
        .WithResourceMapping(
            new FileInfo(Path.Combine(InitDirectory, "01_schema_and_seed.sql")),
            "/docker-entrypoint-initdb.d/")
        .Build();

    public async ValueTask InitializeAsync() => await _postgres.StartAsync();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString())
        {
            Username = "accounts_app",
            Password = null
        }.ConnectionString;

        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:AccountsDb", connectionString);
        builder.UseSetting("Database:AppPassword", AppPassword);
        builder.UseSetting("AccountsApiKey", TestApiKey);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }


    /// <summary>
    /// Creates a gRPC channel to the in-memory test server. Wrap it in any generated client.
    /// </summary>
    public GrpcChannel CreateGrpcChannel() =>
        GrpcChannel.ForAddress(Server.BaseAddress, new GrpcChannelOptions
        {
            HttpHandler = Server.CreateHandler()
        });

    /// <summary>
    /// The API key header every call needs.
    /// </summary>
    public Metadata ApiKey()
    {
        var headers = new Metadata();
        headers.Add("x-api-key", CustomWebApplicationFactory.TestApiKey);
        return headers;
    }
}