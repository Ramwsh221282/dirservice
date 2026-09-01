using DirectoryService.Infrastructure.Identity.BackgroundServices;
using DirectoryService.Infrastructure.Identity.Commands.Logout;
using DirectoryService.Infrastructure.Identity.Commands.RefreshToken;
using DirectoryService.Infrastructure.Identity.Commands.SignIn;
using DirectoryService.Infrastructure.Identity.Commands.SignUp;
using DirectoryService.Infrastructure.Identity.Database;
using DirectoryService.Infrastructure.Identity.Hashing;
using DirectoryService.Infrastructure.Identity.Jwt;
using DirectoryService.Infrastructure.Identity.Repositories;
using DirectoryService.UseCases.Common.Cqrs;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Infrastructure.Identity.DependencyInjection;

public static class IdentityDependencyInjectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        string connectionString,
        JwtOptions jwtOptions
    )
    {
        services.AddSingleton(jwtOptions);
        services.AddSingleton<IIdentityConnectionFactory>(_ => new NpgSqlIdentityConnectionFactory(connectionString));
        services.AddSingleton<IIdentityTransactionSource, NpgSqlIdentityTransactionSource>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();
        services.AddSingleton<ITokenValidator, JwtTokenValidator>();
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<ISessionsRepository, SessionsRepository>();

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
}
