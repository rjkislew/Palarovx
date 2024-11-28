using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Palaro2026.Context;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register TokenService
builder.Services.AddScoped<TokenService>();

// Configure the database context
var connectionString = builder.Configuration.GetConnectionString("Palaro2026")
                       ?? throw new InvalidOperationException("Connection string is not configured properly.");

builder.Services.AddDbContext<Palaro2026Context>(options =>
    options.UseSqlServer(connectionString)
);

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

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
app.UseAuthentication(); // Ensure authentication is used
app.UseAuthorization(); // Ensure authorization is used

app.MapControllers(); // Map controller routes
app.Run(); // Run the application