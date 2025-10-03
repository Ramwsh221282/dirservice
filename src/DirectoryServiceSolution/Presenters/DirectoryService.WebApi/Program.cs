using DirectoryService.Infrastructure.PostgreSQL.EntityFramework;
using DirectoryService.Infrastructure.PostgreSQL.EntityFramework.Repositories.Locations;
using DirectoryService.Infrastructure.PostgreSQL.Options;
using DirectoryService.UseCases.Locations.Contracts;
using DirectoryService.UseCases.Locations.CreateLocation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services.AddOptions<NpgSqlConnectionOptions>()
    .Bind(builder.Configuration.GetSection(nameof(NpgSqlConnectionOptions)));
builder.Services.AddScoped<CreateLocationCommandHandler>();
builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();

builder.Services.AddScoped<ServiceDbContext>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapSwagger();

app.Run();

namespace DirectoryService.API
{
    public partial class Program;
}
