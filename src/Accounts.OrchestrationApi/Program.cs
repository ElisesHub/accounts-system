using AccountsAPI.Services;
using AccountsApplicationAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Infrastructure.ExternalClients;
using PortfolioApplicationAPI.Infrastructure.Security;
using PortfolioApplicationAPI.Presentation.Authentication;
using PortfolioApplicationAPI.Presentation.ExceptionHandling;
using PortfolioApplicationAPI.Presentation.Models;
using PortfolioApplicationAPI.Presentation.Models.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.SchemeName)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.SchemeName,
        options => { options.HeaderName = "x-api-key"; });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireApiKey", policy =>
    {
        policy.AuthenticationSchemes.Add(ApiKeyAuthenticationOptions
            .SchemeName);
        policy.RequireAuthenticatedUser();
    });
});
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage)
                        .ToArray()
                );

            var response = new ApiErrorResponse
            {
                Code = ApiErrorCodes.ValidationError.ToString(),
                Message = "One or more validation errors occurred.",
                FieldErrors = errors,
                TraceId = context.HttpContext.TraceIdentifier
            };

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5253");
});

var app = builder.Build();
app.UseExceptionHandler();
app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




app.Run();