using Microsoft.EntityFrameworkCore;
using ResourceManager.Infrastructure;
using ResourceManager.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("ConnectionStrings:Database is not set.");
}

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<ResourceRepository>();

builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

// The integration tests reach the application through WebApplicationFactory<Program>, and
// that needs Program to be a public type.
public partial class Program
{
}
