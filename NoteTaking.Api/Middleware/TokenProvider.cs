using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace NoteTaking.Api.Middleware;

public sealed class TokenProvider(IConfiguration configuration)
{
    public async Task<string> GenerateToken()
    {
        
        var tokenUser = configuration.GetSection("JwT-User").Get<TokenUser>();
        string key = configuration["JwT-Secrets"]!;
        var SecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);

        var TokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim("Username", tokenUser!.UserName.ToString()),
                new Claim("Password", tokenUser.Password)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(configuration.GetValue<int>("JwT:ExpirationInMinutes")),
            SigningCredentials = credentials,
            Issuer = configuration["JwT:Issuer"],
            Audience = configuration["JwT:Audience"]
        };

        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(TokenDescriptor);

        return await Task.FromResult(token);
    }
}
