using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OrchestrationApi.Infrastructure.ExternalClients.Auth0;
using OrchestrationApi.Infrastructure.ExternalClients.Configuration;
using Xunit;

namespace OrchestrationApi.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting($"{ExternalApiOptions.SectionName}:BaseUrl", "https://localhost:5001");
        builder.UseSetting($"{Auth0Options.SectionName}:Domain", "test.example");
        builder.UseSetting($"{Auth0Options.SectionName}:ClientId", "test-client-id");
        builder.UseSetting($"{Auth0Options.SectionName}:ClientSecret", "test-client-secret");
        builder.UseSetting("AccountsApplicationApiKey", "test-api-key");

    }


}