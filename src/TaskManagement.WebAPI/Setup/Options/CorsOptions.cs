using System.Collections.Generic;

namespace TaskManagement.WebAPI.Setup.Options;

/// <summary>
/// Allowed browser origins for CORS, bound from configuration.
/// </summary>
public class CorsOptions
{
    public const string SectionName = "Cors";

    public List<string> AllowedOrigins { get; set; } = [];
}
