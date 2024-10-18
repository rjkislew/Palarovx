using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure the database context based on the environment
var environment = builder.Environment.EnvironmentName;
string connectionString;

if (environment == "Development")
{
    connectionString = builder.Configuration.GetConnectionString("palaro_2026DevelopmentConnection");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("palaro_2026ProductionConnection");
}

builder.Services.AddDbContext<Server.Palaro2026.Context.palaro_2026Context>(
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
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "Palaro 2026 API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
