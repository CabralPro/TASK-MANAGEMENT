using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace TaskManagement.WebAPI.Setup.Logging;

/// <summary>
/// Shared Serilog output settings for console logging.
/// </summary>
public static class SerilogFormatting
{
    public const string ConsoleOutputTemplate =
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{ClassName}] {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration WriteStyledConsole(this LoggerConfiguration configuration) =>
        configuration.WriteTo.Console(
            outputTemplate: ConsoleOutputTemplate,
            theme: AnsiConsoleTheme.Code,
            applyThemeToRedirectedOutput: true);
}
