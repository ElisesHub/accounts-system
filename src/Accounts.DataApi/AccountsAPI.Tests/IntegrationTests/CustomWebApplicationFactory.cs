

using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AccountsAPI.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var testSettings = new Dictionary<string, string>
            {
                // TODO: Change to test database
                ["ConnectionStrings:AccountsDb"] =
                    "Server=localhost;Database=AccountsDb;Uid=accounts_user;Pwd=REMOVED;"
            };

            configBuilder.AddInMemoryCollection(testSettings);
        });
    }
}