using System.Reflection;
using System.Text;
using ChatForge;
using ChatForge.DataAccess;
using ChatForge.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public class Program()
{
    
    
    public static void Main(string[] args)
    {
        
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://*:8211");
        
        // Add services to the container.
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddSingleton<ChatForgeContext>();
        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<IChatroomService, ChatroomService>();
        builder.Services.AddSingleton<TokenService>();
        builder.Services.AddSignalR();
        builder.Services.AddSingleton<ForgeHub>();
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding
                        .UTF8
                        .GetBytes(Environment
                            .GetEnvironmentVariable("CHATFORGEKEY")!)),
            
                    ValidateIssuer = true,
                    ValidIssuer = "ChatForge",
            
                    ValidateAudience = true,
                    ValidAudience = "ChatForgeFrontend",
            
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
            
                    NameClaimType = "UserId"
                };
            });
        builder.Services.AddControllers();
        foreach (var model in GetTypes("ChatForge.Models", Assembly.GetExecutingAssembly()))
        {
            builder.Services.AddSingleton(model);
            builder.Services.AddSingleton(typeof(Repository<>).MakeGenericType(model));
        }
        

        var app = builder.Build();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();
        
        #pragma warning disable ASP0014
        app.UseEndpoints(ep =>
        {
            ep.MapHub<ForgeHub>("/hub");

        });
        #pragma warning restore ASP0014
        
        app.Run();
        
    }
    
    public static IEnumerable<Type> GetTypes(string namespaceName, Assembly assembly)
    {

        var types = assembly.GetTypes()
            .Where(t => t.Namespace == namespaceName
                        && t.IsClass
                        && !t.IsNested
                        && !t.IsAbstract
            );

        return types;
        
    }
    
}
