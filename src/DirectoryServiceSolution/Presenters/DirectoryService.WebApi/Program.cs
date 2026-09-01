using DirectoryService.Infrastructure.PostgreSQL.Migrations;
using DirectoryService.Infrastructure.PostgreSQL.Seeding;
using DirectoryService.WebApi.Configurations;
using DirectoryService.WebApi.DependencyInjection;
using DirectoryService.WebApi.Middlewares;
using DirectoryService.WebApi.Seeding;

Serilog.Log.Logger.Information("Application is starting...");
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Serilog.Log.Logger.Information("Resolving `ASPNETCORE_ENVIRONMENT` environment variable");
string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower();
if (string.IsNullOrWhiteSpace(environment))
{
    throw new ApplicationException(
        """
        ASPNETCORE_ENVIRONMENT environment variable is not set.
        Please specify.
        `ASPNETCORE_ENVIRONMENT=development`
        or
        `ASPNETCORE_ENVIRONMENT=production`
        """);
}

ApplicationConfig config = environment switch
{
    "development" => ApplicationConfig.CreateForDevelopment(".env"),
    "production" => ApplicationConfig.CreateFromEnvironment(),
    _ => throw new ApplicationException(string.Format(
        """
        Unsupported `ASPNETCORE_ENVIRONMENT` environment variable value: {0}
        Please specify
        `ASPNETCORE_ENVIRONMENT=development`
        or
        `ASPNETCORE_ENVIRONMENT=production`.
        """)),
};

Serilog.Log.Logger.Information("Resoled {0} `ASPNETCORE_ENVIRONMENT` environment variable", environment);

builder.Services.AddSingleton<DatabaseConfig>(config.Database);
builder.Services.AddSingleton<SeqConfig>(config.Seq);

builder.AddSeqLogging(config);
builder.InjectInfrastructureLayers(config);
builder.InjectUseCaseLayer();
builder.AddIdentity(config);
builder.AddMigrations(config);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.AddConfiguredSwagger();

builder.Services.AddScoped<ISeeder, LocationsSeeder>();
builder.Services.AddScoped<ISeeder, DepartmentsSeeder>();
builder.Services.AddScoped<ISeeder, PositionsSeeder>();
builder.Services.AddScoped<ISeeder, UsersSeeder>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        CorsConfig.PolicyName,
        policy =>
        {
            if (config.Cors.AllowedOrigins.Count == 0)
            {
                return;
            }

            policy
                .WithOrigins([.. config.Cors.AllowedOrigins])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

Serilog.Log.Logger.Information("Services are configured", environment);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "DirectoryService API v1");
    options.DocumentTitle = "DirectoryService API";
});
app.MapOpenApi();

Serilog.Log.Logger.Information("Applying database migrations");
app.Services.ApplySqlFileMigrations();
Serilog.Log.Logger.Information("Database migrations are applied");

if (config.Seed.UseSeed)
{
    Serilog.Log.Logger.Information("Seeding is required. Starting up database seeding");
    await app.Services.RunSeeders();
}
else
{
    Serilog.Log.Logger.Information("Seeding is not required. Skipping database seeding");
}

app.UseExceptionHandleMiddleware();
app.UseRouting();
app.UseCors(CorsConfig.PolicyName);
app.UseAuthenticationMiddleware();
app.MapControllers();
app.MapSwagger();

Serilog.Log.Logger.Information("Starting up");
app.Run();

namespace DirectoryService.WebApi
{
    public partial class Program;
}
