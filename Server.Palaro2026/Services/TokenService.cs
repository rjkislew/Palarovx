using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string username, string userId)
    {
        var claims = new[]
        {
        new Claim(ClaimTypes.Name, username),
        new Claim("User Id", userId), // Unique user identifier
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Ensure a unique identifier
        new Claim("LoginTimestamp", DateTime.UtcNow.ToString("o")), // Add a timestamp claim
        new Claim("Nonce", Guid.NewGuid().ToString()) // Add a random value for uniqueness
    };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30), // Set expiration
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}