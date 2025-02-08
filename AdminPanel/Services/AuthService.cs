namespace AdminPanel.Services;

using AdminPanel.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class AuthService : IAuthService
{
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromHours(3);
    private readonly string? _secretKey;
    private readonly string? _issuer;
    private readonly string? _audience;
    private readonly byte[] _keyBytes;

    public AuthService()
    {
        (_secretKey, _issuer, _audience) = GetJwtConfig();
        _keyBytes = Encoding.ASCII.GetBytes(_secretKey ?? string.Empty);
    }

    private (string? secretKey, string? issuer, string? audience) GetJwtConfig()
    {
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            //TODO: Log error
            System.Console.WriteLine($"Secret: {secretKey}\nIssuer: {issuer}\nAudience: {audience}");
            throw new InvalidOperationException("JWT configuration is missing within the environment variables");
        }

        return (secretKey, issuer, audience);
    }

    private TokenValidationParameters GetTokenValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_keyBytes),
            ValidateIssuer = true,
            ValidIssuer = _issuer,
            ValidateAudience = true,
            ValidAudience = _audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    public void AddAuth(IServiceCollection services)
    {
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = GetTokenValidationParameters();
            });
    }

    public string? GenerateToken(string username)
    {
        if (string.IsNullOrEmpty(_secretKey)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User")
            }),
            Expires = DateTime.UtcNow.Add(_tokenLifetime),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
    public string? GenerateAdminToken(string username)
    {
        if (string.IsNullOrEmpty(_secretKey)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")  // Set role as Admin
            }),
            Expires = DateTime.UtcNow.Add(_tokenLifetime),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string? RenewToken(string token)
    {
        var principal = ValidateToken(token);
        if (principal == null)
        {
            return null;
        }

        var username = principal.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrEmpty(username))
        {
            return null;
        }
        return GenerateToken(username);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(_secretKey)) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ValidateToken(token, GetTokenValidationParameters(), out _);
    }
}