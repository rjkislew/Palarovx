using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the database context based on the environment
var environment = builder.Environment.EnvironmentName;
string? connectionString; // Change to nullable type

connectionString = builder.Configuration.GetConnectionString("palaro_2026");

// Check for null and handle it appropriately
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string is not configured properly.");
}

builder.Services.AddDbContext<Server.Palaro2026.Context.Palaro2026Context>(
    options =>
    {
        options.UseSqlServer(connectionString);
    }
);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyMethod()
               .AllowAnyHeader()
               .SetIsOriginAllowed(origin => true) // allow any origin
               .AllowCredentials();
    });
});

var app = builder.Build();

// Use CORS policy
app.UseCors("AllowAll");

// Enable Swagger for all environments
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("v1/swagger.json", "Palaro 2026 API v1");
    options.DefaultModelsExpandDepth(-1);
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();