using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Server.Palaro2026.Context;
using Scalar.AspNetCore;
using System.Text;
using Server.Palaro2026;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.MaxValidationDepth = 64; // Adjust this value as needed
});
builder.Services.AddEndpointsApiExplorer();

// Configure the database context
var connectionString = builder.Configuration.GetConnectionString("Palaro2026DB")
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
                  "https://localhost",
                  "https://pgas.ph",
                  "https://localhost:7061",
                  "https://localhost:7170",
                  "https://localhost:7169"
              }.Contains(origin))
              .AllowCredentials());
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = jwtSettings["Key"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
    options.IncludeErrorDetails = true;
});


builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options.Title = "Palaro 2026 API";
        options.ShowSidebar = true;
        options.HideModels = true;
    });
    app.MapOpenApi();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();