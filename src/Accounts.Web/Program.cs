using Auth0.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PortfolioFe.Application.Interfaces;
using PortfolioFe.Application.Services;
using PortfolioFe.Infrastructure.ExternalClients;
using PortfolioFe.Infrastructure.ExternalClients.Configuration;
using PortfolioFe.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Read Docker/container-mounted secrets from /run/secrets.
// // Each file name becomes a configuration key; each file's contents become the value.
builder.Configuration.AddKeyPerFile(
    directoryPath: "/run/secrets",
    optional: true);


builder.Services.AddAuth0WebAppAuthentication(options =>
{
    options.Domain = builder.Configuration["Auth0:Domain"] ??
                     throw new InvalidOperationException(
                         "Missing Auth0:Domain in configuration");
    options.ClientId = builder.Configuration["Auth0:ClientId"] ??
                       throw new InvalidOperationException(
                           "Missing Auth0:ClientId in configuration");
    options.ClientSecret = builder.Configuration["Auth0:ClientSecret"] ??
                           throw new InvalidOperationException(
                               "Missing Auth0:ClientSecret in configuration");
});


builder.Services.AddOptions<ApiKeyOptions>()
    .Bind(builder.Configuration)
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(options.AccountsApplicationApiKey),
        "One or more API keys are missing.")
    .ValidateOnStart();

builder.Services.AddOptions<ExternalApiOptions>()
    .Bind(builder.Configuration.GetSection(ExternalApiOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl),
        "External API base URL is missing.")
    .Validate(
        options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
        "ExternalApiOptions:BaseUrl must be a valid absolute URI.")
    .ValidateOnStart();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<IOptions<ExternalApiOptions>>()
        .Value;

    if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
    {
        throw new InvalidOperationException(
            $"ExternalApi:BaseUrl must be a valid absolute URI. Current value: '{options.BaseUrl}'.");
    }

    client.BaseAddress = baseUri;
});
var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
// Handles 404, 403, 500 status codes that are not thrown exceptions
app.UseStatusCodePagesWithReExecute("/Error/StatusCode", "?code={0}");
 // app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();