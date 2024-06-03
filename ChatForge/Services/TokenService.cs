using System.Security.Claims;
using System.Text;
using ChatForge.DataAccess;
using ChatForge.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ChatForge.Services;

public class TokenService
{

    private Repository<ForbiddenToken> _tokens;

    public TokenService(Repository<ForbiddenToken> tokens)
    {
        _tokens = tokens;
    }
    public string GetKey(User user, int expiry = 3, bool isRefresh = false)
    {
        
        var handler = new JsonWebTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "ChatForge",
            Audience = "ChatForgeFrontend",
            Expires = DateTime.UtcNow.AddHours(expiry),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(Encoding
                    .UTF8
                    .GetBytes(Environment
                        .GetEnvironmentVariable("CHATFORGEKEY")!)), SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>()
            {
                { "UserId", user.Id },
            },
        };
        if (isRefresh)
        {
            descriptor.Claims.Add("RefreshKey", "RefreshKey");
        }
        var token = handler.ReadJsonWebToken(handler.CreateToken(descriptor)).EncodedToken;
        return token;
        
    }

    public void ForbidToken(string token)
    {
        var forbiddenToken = new ForbiddenToken();
        forbiddenToken.TokenString = token.Replace("Bearer ", "");
        _tokens.Add(forbiddenToken);
    }
    
    public bool CheckForbid(string token)
    {
        if (_tokens.GetAll().Select(t => t.TokenString).ToList().Contains(token.Replace("Bearer ", "")))
        {
            return true;
        }

        return false;
    }
    
}

