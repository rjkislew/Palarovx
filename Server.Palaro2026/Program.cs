using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Server.Palaro2026.Context;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
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
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // Ensure authentication is used
app.UseAuthorization(); // Ensure authorization is used

app.MapControllers(); // Map controller routes
app.Run(); // Run the application