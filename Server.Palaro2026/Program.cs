using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Server.Palaro2026;
using Server.Palaro2026.Context;
using Server.Palaro2026.Services;
using System.Text;
using System.Text.Json.Serialization;
using OfficeOpenXml;
using Server.Palaro2026.Services.UploadServices;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IPlayerImportService, PlayerImportService>();
builder.Services.AddScoped<ICoachImportService, CoachImportService>();


var connectionString = builder.Configuration.GetConnectionString("Palaro2026DB")
                       ?? throw new InvalidOperationException("Connection string is not configured properly.");


builder.Services.AddDbContext<Palaro2026Context>(options =>
    options.UseSqlServer(connectionString)
);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.WithOrigins(
            "https://palarongpambansa2026.com",
            "https://palarongpambansa2026.com:444",
            "https://palarongpambansa2026.com:446",
            "https://localhost",
            "https://localhost:444",
            "https://localhost:446",
            "https://pgas.ph",
            "https://localhost:7061",
            "https://localhost:7170",
            "https://localhost:7169",
            "https://localhost:7154"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
    );
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? "default-key"))
    };
    options.IncludeErrorDetails = true;
});

builder.Services.AddOpenApi("v1", options => { options.AddDocumentTransformer<BearerSecuritySchemeTransformer>(); });
builder.Services.AddScoped<ISqlDataAccess, SqlDataAccess>();

var app = builder.Build();

// Fallback for first-time execution (avoids null values)
string GetHostAddress = "http://localhost";

app.Use(async (context, next) =>
{
    // Get Host and PathBase dynamically
    var hostAddress = $"{context.Request.Scheme}://{context.Request.Host}";
    var pathBase = context.Request.PathBase.HasValue ? context.Request.PathBase.Value : "";

    // Update Scalar API Server URL dynamically
    GetHostAddress = $"{hostAddress}{pathBase}".TrimEnd('/');

    await next();
});

// Dynamically get Host and PathBase
app.MapScalarApiReference(options =>
{
    options.Title = "Palaro 2026 API";
    options.ShowSidebar = true;
    options.HideModels = true;

    // Host & PathBase will be dynamically determined at runtime
    options.AddServer(GetHostAddress);
});

app.MapOpenApi();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();