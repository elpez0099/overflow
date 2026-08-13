using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Helpers;

public static class AuthExtensions
{
    public static IServiceCollection AddKeycloakAuthentication(this IServiceCollection services)
    {
        services
            .AddAuthentication()
            .AddKeycloakJwtBearer(
                serviceName: "keycloak",
                realm: "overflow",
                options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "overflow";
                    // Cuando  se usa aspire deploy -o infra, el issuer se asume por la red de docker
                    // En ese caso el issuer seria http://keycloak:8080
                    // Sin embargo el issuer real es http://localhost:6001
                    // Para evitar esa discordancia al usar docker-compose le decimos a keycloak quien es el issuer
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuers = [
                            "http://localhost:6001/realms/overflow",
                            "http://keycloak/realms/overflow",
                            "http://id.overflow.local/realms/overflow"
                        ]
                    };
                        
                });
        return services;
    }
}