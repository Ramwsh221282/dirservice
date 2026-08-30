using Serilog;

namespace DirectoryService.WebApi.Configurations;

public sealed class LoggingSeqConfiguration
{
    public string Host { get; set; } = string.Empty;
}

public static class LoggingSeqConfigurationExtension
{
    public static void AddSeqLogging(this WebApplicationBuilder builder, ApplicationConfig config)
    {
        string host = config.Seq.Host;
        Serilog.ILogger logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.Seq(host)
            .CreateLogger();

        ILoggerFactory loggerFactory = LoggerFactory.Create(logBuilder =>
        {
            logBuilder.AddConsole();
        });

        builder.Services.AddSingleton(logger);
        builder.Services.AddSingleton(loggerFactory);
    }
}
