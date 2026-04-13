using AccountsAPI.Services;
using PortfolioApplicationAPI.Application.Interfaces;
using PortfolioApplicationAPI.Infrastructure.ExternalClients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddHttpClient<IExternalAccountsClient, ExternalAccountsClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5253");
});

var app = builder.Build();
app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();




app.Run();