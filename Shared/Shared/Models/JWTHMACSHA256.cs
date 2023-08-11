
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Shared.Models;

public class JwtHmacSha256
{
    private readonly SecurityKey _securityKey;

    private readonly int _expiration;
    
    private const int DefaultExpiration = 86400;

    
    public JwtHmacSha256(string key, int expiration = DefaultExpiration)
    {
        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        _expiration = expiration == 0 ? DefaultExpiration : expiration;
    }

    public string Create()
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Subject = new ClaimsIdentity(new []
            // {
            //     new Claim(ClaimTypes.Name, ""),
            //     new Claim(ClaimTypes.Role, "")
            // }),
            Expires = DateTime.UtcNow.AddSeconds(_expiration),
            SigningCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public bool Verify(string tokenString)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var success = false;
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _securityKey,
            ClockSkew = TimeSpan.Zero,
            ValidateAudience = false,
            ValidateIssuer = false,
        };

        try
        {
            tokenHandler.ValidateToken(tokenString, validationParameters, out SecurityToken validatedToken);
            success = true;
        }
        catch (SecurityTokenExpiredException ex)
        {
            Console.WriteLine(ex);
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine(ex);
        }

        return success;
    }
}