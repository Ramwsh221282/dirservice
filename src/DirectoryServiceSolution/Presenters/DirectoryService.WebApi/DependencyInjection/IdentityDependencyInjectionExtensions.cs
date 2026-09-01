using DirectoryService.Infrastructure.Identity.DependencyInjection;
using DirectoryService.Infrastructure.Identity.Jwt;
using DirectoryService.WebApi.Configurations;

namespace DirectoryService.WebApi.DependencyInjection;

public static class IdentityDependencyInjectionExtensions
{
    public static void AddIdentity(this WebApplicationBuilder builder, ApplicationConfig config)
    {
        string connectionString = string.Format("Host={0};Port={1};Username={2};Password={3};Database={4}",
            config.Database.HostName,
            config.Database.Port,
            config.Database.UserName,
            config.Database.Password,
            config.Database.DatabaseName);

        JwtOptions jwtOptions = new()
        {
            SecretKey = config.Identity.JwtHashkey,
        };

        builder.Services.AddIdentityInfrastructure(connectionString, jwtOptions);
    }
}
