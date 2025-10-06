using DirectoryService.WebApi.Configurations;
using DirectoryService.WebApi.DependencyInjection;
using DirectoryService.WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.InjectUseCaseLayer();
builder.InjectPostgreSqlLayer();
builder.AddSeqLogging();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseExceptionHandleMiddleware();
app.UseHttpsRedirection();
app.MapControllers();
app.MapSwagger();
app.Run();

namespace DirectoryService.WebApi
{
    public partial class Program;
}
