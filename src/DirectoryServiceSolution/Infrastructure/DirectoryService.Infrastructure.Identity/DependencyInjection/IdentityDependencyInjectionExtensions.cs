using Dapper;
using DirectoryService.Infrastructure.Identity.BackgroundServices;
using DirectoryService.Infrastructure.Identity.Commands.Logout;
using DirectoryService.Infrastructure.Identity.Commands.RefreshToken;
using DirectoryService.Infrastructure.Identity.Commands.SignIn;
using DirectoryService.Infrastructure.Identity.Commands.SignUp;
using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Hashing;
using DirectoryService.Infrastructure.Identity.Jwt;
using DirectoryService.Infrastructure.Identity.Options;
using DirectoryService.Infrastructure.Identity.Repositories;
using DirectoryService.UseCases.Common.Cqrs;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DirectoryService.Infrastructure.Identity.DependencyInjection;

public static class IdentityDependencyInjectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IdentityConnectionOptions options,
        JwtOptions jwtOptions
    )
    {
        SqlMapper.AddTypeHandler(new GuidTypeHandler());

        services.AddSingleton(options);
        services.AddSingleton(jwtOptions);
        services.AddSingleton<IIdentityConnectionFactory, SqliteIdentityConnectionFactory>();
        services.AddSingleton<IIdentityTransactionSource, SqliteIdentityTransactionSource>();
        services.AddSingleton<IdentitySchemaInitializer>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddScoped<IIdentityRepository, SqliteIdentityRepository>();
        services.AddScoped<ISessionsRepository, SqliteSessionsRepository>();

        services.AddScoped<ICommandHandler<Guid, SignUpCommand>, SignUpCommandHandler>();
        services.AddScoped<ICommandHandler<SignInResult, SignInCommand>, SignInCommandHandler>();
        services.AddScoped<ICommandHandler<RefreshTokenResult, RefreshTokenCommand>, RefreshTokenCommandHandler>();
        services.AddScoped<ICommandHandler<LogoutCommand>, LogoutCommandHandler>();

        services.AddScoped<IValidator<SignUpCommand>, SignUpCommandValidator>();
        services.AddScoped<IValidator<SignInCommand>, SignInCommandValidator>();
        services.AddScoped<IValidator<RefreshTokenCommand>, RefreshTokenCommandValidator>();
        services.AddScoped<IValidator<LogoutCommand>, LogoutCommandValidator>();

        services.AddHostedService<ExpiredAccessTokenCleanupService>();

        return services;
    }

    public static IServiceCollection EnsureIdentitySchemaCreated(this IServiceCollection services)
    {
        using ServiceProvider provider = services.BuildServiceProvider();
        IdentitySchemaInitializer initializer = provider.GetRequiredService<IdentitySchemaInitializer>();
        initializer.EnsureCreated();

        return services;
    }
}
