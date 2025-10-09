using System.Reflection;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Departments;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Locations;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Positions;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using DirectoryService.UseCases.Common.Cqrs;
using DirectoryService.UseCases.Common.Transaction;
using DirectoryService.UseCases.Common.UnitOfWork;
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
        builder.Services.AddValidatorsFromAssembly(typeof(ICommand).Assembly);
    }

    public static void InjectPostgreSqlLayer(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<NpgSqlConnectionOptions>()
            .Bind(builder.Configuration.GetSection(nameof(NpgSqlConnectionOptions)));
        builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();
        builder.Services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        builder.Services.AddScoped<ServiceDbContext>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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

    private static bool ImplementsHandler(this Type type) =>
        type.IsGenericType
        && (
            type.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
            || type.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
        );
}
