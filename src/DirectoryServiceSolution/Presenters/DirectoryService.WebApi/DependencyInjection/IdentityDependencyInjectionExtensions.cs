using DirectoryService.Infrastructure.Identity.DependencyInjection;
using DirectoryService.Infrastructure.Identity.Jwt;
using DirectoryService.Infrastructure.Identity.Options;
using DirectoryService.WebApi.Configurations;

namespace DirectoryService.WebApi.DependencyInjection;

public static class IdentityDependencyInjectionExtensions
{
    public static void AddIdentity(this WebApplicationBuilder builder, ApplicationConfig config)
    {
        IdentityConnectionOptions connectionOptions = new()
        {
            ConnectionString = config.Identity.IdentityDbConnectionString,
        };

        JwtOptions jwtOptions = new()
        {
            SecretKey = config.Identity.JwtHashkey,
        };

        builder.Services.AddIdentityInfrastructure(connectionOptions, jwtOptions);
    }
}
