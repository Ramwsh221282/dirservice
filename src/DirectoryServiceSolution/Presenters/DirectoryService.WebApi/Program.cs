using DirectoryService.Infrastructure.PostgreSQL.Seeding;
using DirectoryService.WebApi.Configurations;
using DirectoryService.WebApi.DependencyInjection;
using DirectoryService.WebApi.Middlewares;

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
    "development" => ApplicationConfig.CreateFromEnvFile(".env"),
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

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ISeeder, LocationsSeeder>();
builder.Services.AddScoped<ISeeder, DepartmentsSeeder>();
builder.Services.AddScoped<ISeeder, PositionsSeeder>();

Serilog.Log.Logger.Information("Services are configured", environment);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();

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
app.UseHttpsRedirection();
app.MapControllers();
app.MapSwagger();

Serilog.Log.Logger.Information("Starting up");
app.Run();

namespace DirectoryService.WebApi
{
    public partial class Program;
}
