using Serilog.Core;
using Serilog.Events;

namespace TaskManagement.WebAPI.Setup.Logging;

/// <summary>
/// Adds a short <c>ClassName</c> property from Serilog's <c>SourceContext</c>.
/// </summary>
public sealed class ClassNameEnricher : ILogEventEnricher
{
    public const string PropertyName = "ClassName";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var sourceContextProperty))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(PropertyName, "App"));
            return;
        }

        var sourceContext = sourceContextProperty.ToString().Trim('"');
        var className = sourceContext.Contains('.')
            ? sourceContext[(sourceContext.LastIndexOf('.') + 1)..]
            : sourceContext;

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(PropertyName, className));
    }
}
