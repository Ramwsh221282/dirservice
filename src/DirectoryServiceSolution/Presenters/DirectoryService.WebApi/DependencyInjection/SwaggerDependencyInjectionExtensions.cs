using System.Reflection;
using DirectoryService.WebApi.Middlewares;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DirectoryService.WebApi.DependencyInjection;

public static class SwaggerDependencyInjectionExtensions
{
    public static void AddConfiguredSwagger(this WebApplicationBuilder builder)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "DirectoryService API",
                    Version = "v1",
                    Description =
                        "API справочника организационной структуры: подразделения, локации и должности.\n\n"
                        + "**Авторизация.** Все эндпоинты, кроме `/api/auth/sign-up`, `/api/auth/sign-in`, "
                        + "`/api/auth/refresh` и `/api/auth/logout`, требуют access-токен.\n\n"
                        + "**Тестовые пользователи** (создаются при `USE_SEED=true`): "
                        + "`admin` / `admin12345`, `manager` / `manager12345`, `employee` / `employee12345`.",
                }
            );

            OpenApiSecurityScheme securityScheme = new()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Access-токен из ответа `POST /api/auth/sign-in` (поле `value.token.accessToken`). "
                    + "Вставьте только сам токен — префикс `Bearer` подставляется автоматически.",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            };

            options.AddSecurityDefinition("Bearer", securityScheme);
            options.OperationFilter<AuthenticationOperationFilter>();

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }
        });
    }
}

public sealed class AuthenticationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        bool requiresAuthentication =
            context.MethodInfo.DeclaringType?.GetCustomAttribute<RequiresAuthenticationAttribute>()
                != null
            || context.MethodInfo.GetCustomAttribute<RequiresAuthenticationAttribute>() != null;

        bool allowsAnonymous =
            context.MethodInfo.DeclaringType?.GetCustomAttribute<AllowAnonymousAccessAttribute>()
                != null
            || context.MethodInfo.GetCustomAttribute<AllowAnonymousAccessAttribute>() != null;

        if (!requiresAuthentication || allowsAnonymous)
        {
            return;
        }

        OpenApiSecurityScheme scheme = new()
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer",
            },
        };

        operation.Security = [new OpenApiSecurityRequirement { [scheme] = [] }];

        operation.Responses.TryAdd(
            "401",
            new OpenApiResponse { Description = "Access-токен отсутствует, истёк или недействителен." }
        );
    }
}
