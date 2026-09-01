using System.Data;
using System.Reflection;
using DirectoryService.Infrastructure.PostgreSQL.Database;
using DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Departments;
using DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Locations;
using DirectoryService.Infrastructure.PostgreSQL.Database.Repositories.Positions;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Database;
using DirectoryService.UseCases.Common.Transaction;
using DirectoryService.UseCases.Departments.Contracts;
using DirectoryService.UseCases.Locations.Contracts;
using DirectoryService.UseCases.Positions.Contracts;
using FluentValidation;

namespace DirectoryService.WebApi.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static void InjectUseCaseLayer(this WebApplicationBuilder builder)
    {
        Assembly assembly = typeof(ICommand<>).Assembly;
        builder.Services.InjectUseCaseHandlers(assembly);
        builder.Services.InjectQueryHandlers(assembly);
        builder.Services.AddValidatorsFromAssembly(typeof(ICommand).Assembly);
    }

    public static void InjectInfrastructureLayers(this WebApplicationBuilder builder, ApplicationConfig config)
    {
        string connectionString = string.Format("Host={0};Port={1};Username={2};Password={3};Database={4}",
            config.Database.HostName,
            config.Database.Port,
            config.Database.UserName,
            config.Database.Password,
            config.Database.DatabaseName);
        NpgSqlConnectionOptions options = new() { ConnectionString = connectionString };
        builder.Services.AddSingleton<NpgSqlConnectionOptions>(options);
        builder.Services.AddSingleton<IDbConnectionFactory, NpgSqlConnectionFactory>();
        builder.Services.AddScoped<IDbConnection>(sp =>
            sp.GetRequiredService<IDbConnectionFactory>().Create().GetAwaiter().GetResult()
        );
        builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
        builder.Services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        builder.Services.AddScoped<IDepartmentLocationsRepository, DepartmentLocationsRepository>();
        builder.Services.AddScoped<IDepartmentPositionsRepository, DepartmentPositionsRepository>();
        builder.Services.AddScoped<ITransactionSource, TransactionSource>();
        builder.Services.AddScoped<IPositionsRepository, PositionsRepository>();
    }

    public static T GetService<T>(this AsyncServiceScope scope)
        where T : notnull
    {
        T service = scope.ServiceProvider.GetRequiredService<T>();
        return service;
    }

    private static void InjectUseCaseHandlers(this IServiceCollection services, Assembly assembly)
    {
        IEnumerable<Type> implementations = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(ImplementsHandler));

        foreach (Type implementation in implementations)
        {
            Type handlerInterface = implementation.GetInterfaces().Single(ImplementsHandler);
            services.AddScoped(handlerInterface, implementation);
        }
    }

    private static void InjectQueryHandlers(this IServiceCollection services, Assembly assembly)
    {
        IEnumerable<Type> implementations = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t => t.GetInterfaces().Any(ImplementsQueryHandler));

        foreach (Type implementation in implementations)
        {
            Type handlerInterface = implementation.GetInterfaces().Single(ImplementsQueryHandler);
            services.AddScoped(handlerInterface, implementation);
        }
    }

    private static bool ImplementsHandler(this Type type)
    {
        return type.IsGenericType
        && (
            type.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
            || type.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
        );
    }

    private static bool ImplementsQueryHandler(this Type type)
    {
        return type.IsGenericType
        && (
            type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
            || type.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)
        );
    }
}
