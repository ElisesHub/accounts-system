using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using DotNet.Testcontainers.Configurations;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace AccountsAPI.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{

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
        builder.UseSetting("Auth0:Audience", TestTokens.Audience);
        builder.UseSetting("Auth0:Domain", "TestDomain");


        builder.ConfigureTestServices(services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.ConfigurationManager = null;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = TestTokens.Issuer,
                    ValidAudience = TestTokens.Audience,
                    IssuerSigningKey = TestTokens.SigningKey
                };
            });
        });
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
    /// Authorization metadata for a gRPC call. Defaults to a valid token with read:accounts;
    /// pass a token from TestTokens.Create(...) to test other cases.
    /// </summary>
    public Metadata Bearer(string? token = null)
    {
        var headers = new Metadata();
        headers.Add("Authorization", $"Bearer {token ?? TestTokens.Create()}");
        return headers;
    }
}