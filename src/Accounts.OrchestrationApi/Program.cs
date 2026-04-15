using AccountsAPI.Services;
using AccountsApplicationAPI.Application.Interfaces;
using AccountsApplicationAPI.Infrastructure.ExternalClients;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Infrastructure.Security;
using PortfolioApplicationAPI.Presentation.ExceptionHandling;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
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