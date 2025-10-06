using System.Reflection;
using DirectoryService.UseCases.Common.Cqrs;
using FluentValidation;

namespace DirectoryService.WebApi.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static void InjectUseCaseLayer(this IServiceCollection services)
    {
        Assembly assembly = typeof(ICommand<>).Assembly;
        services.InjectUseCaseHandlers(assembly);
        services.InjectUseCaseValidators(assembly);
    }

    private static void InjectUseCaseValidators(this IServiceCollection services, Assembly assembly)
    {
        IEnumerable<Type> implementations = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .Where(t =>
                t.BaseType != null
                && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>)
            );

        foreach (Type implementation in implementations) { }
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

    private static bool InheritsAbstractValidator(this Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(AbstractValidator<>);
    }

    private static bool ImplementsHandler(this Type type) =>
        type.IsGenericType
        && (
            type.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
            || type.GetGenericTypeDefinition() == typeof(ICommandHandler<>)
        );
}
