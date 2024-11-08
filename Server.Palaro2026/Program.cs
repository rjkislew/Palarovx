using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the database context
var connectionString = builder.Configuration.GetConnectionString("palaro_2026")
                       ?? throw new InvalidOperationException("Connection string is not configured properly.");

builder.Services.AddDbContext<Palaro2026Context>(options =>
    options.UseSqlServer(connectionString)
);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => new[]
              {
                  "https://pgas.ph",
                  "https://localhost:7169",
                  "https://localhost:7063"
              }.Contains(origin))
              .AllowCredentials());
});

// Build the application
var app = builder.Build();

// Middleware pipeline
app.UseCors("AllowAll");
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
