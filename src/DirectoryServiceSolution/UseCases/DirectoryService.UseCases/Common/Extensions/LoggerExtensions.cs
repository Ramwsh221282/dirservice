using ResultLibrary;
using Serilog;

namespace DirectoryService.UseCases.Common.Extensions;

public static class LoggerExtensions
{
    public static ILogger BindTo<T>(this ILogger logger)
    {
        return logger.ForContext<T>();
    }

    public static void LogError(this ILogger logger, Error error)
    {
        string message = error.Message;
        logger.Error("Error: {Error}", message);
    }

    public static void LogError(this ILogger logger, Result result)
    {
        if (result.IsSuccess)
            return;
        logger.LogError(result.Error);
    }

    public static void LogError<T>(this ILogger logger, Result<T> result)
    {
        if (result.IsSuccess)
            return;
        logger.LogError(result.Error);
    }

    public static Error ReturnLogged(this ILogger logger, Error error)
    {
        logger.LogError(error);
        return error;
    }

    public static Result ReturnLogged(this ILogger logger, Result result)
    {
        logger.LogError(result);
        return result;
    }

    public static Result<T> ReturnLogged<T>(this ILogger logger, Result<T> result)
    {
        logger.LogError<T>(result);
        return result;
    }
}
