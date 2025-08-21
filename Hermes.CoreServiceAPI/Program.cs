using Hermes.CoreServiceAPI.Components;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using System.Text;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // -------------------------------
        // Database configuration
        // -------------------------------
        var DBconnectionString = builder.Configuration.GetConnectionString("hermes_db_shop");
        if (string.IsNullOrEmpty(DBconnectionString))
        {
            throw new Exception("La chaîne de connexion à la base de données n'est pas définie !");
        }

        // Enregistrement du DbContext
        builder.Services.AddDbContext<Hermes.CoreServiceAPI.ProductWebsiteContext>(options =>
            options.UseNpgsql(DBconnectionString)
        );

        // -------------------------------
        // Services
        // -------------------------------

        // Blazor (Razor Components)
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // API Controllers
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles
            );

        // JWT Authentication
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };
            });

        // Swagger / NSwag
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(document =>
        {
            document.Title = "Hermes API";
            document.Version = "v1";
            document.AddSecurity("JWT", Enumerable.Empty<string>(),
                new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}."
                }
            );
        });

        var app = builder.Build();

        app.UseHttpsRedirection();

        // Swagger
        app.UseOpenApi();
        app.UseSwaggerUi();

        // Sécurité
        app.UseAuthentication();
        app.UseAuthorization();

        // Antiforgery (Blazor)
        app.UseAntiforgery();

        // Static files + Blazor
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        // API endpoints
        app.MapControllers();

        app.Run();
    }
}
