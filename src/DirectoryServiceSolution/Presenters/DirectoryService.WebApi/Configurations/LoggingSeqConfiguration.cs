using Serilog;

namespace DirectoryService.WebApi.Configurations;

public sealed class LoggingSeqConfiguration
{
    public string Host { get; set; } = string.Empty;
}

public static class LoggingSeqConfigurationExtension
{
    public static void AddSeqLogging(this WebApplicationBuilder builder)
    {
        IConfigurationSection section = builder.Configuration.GetSection(
            nameof(LoggingSeqConfiguration)
        );
        IConfigurationSection hostSection = section.GetSection("Host");
        string? host = hostSection.Value;
        if (string.IsNullOrWhiteSpace(host))
            throw new ApplicationException("Seq hostname was not provided.");

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
