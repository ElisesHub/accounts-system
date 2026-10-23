using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MySql;
using Xunit;

namespace AccountsAPI.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestApiKey = "integration-test-api-key";

    private static readonly string DumpFile =
        Path.Combine(AppContext.BaseDirectory, "TestData",
            "accountsdb_dump_v1.sql");

    private readonly MySqlContainer _mySql = new MySqlBuilder("mysql:8.0")
        .WithDatabase("AccountsDb")
        .WithUsername("accounts_user")
        .WithPassword("integration-test-password")
        .WithResourceMapping(new FileInfo(DumpFile), "/docker-entrypoint-initdb.d/")
        .Build();

    public async ValueTask InitializeAsync() => await _mySql.StartAsync();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:AccountsDb", _mySql.GetConnectionString());
        builder.UseSetting("AccountsApiKey", TestApiKey);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _mySql.DisposeAsync();
    }


}