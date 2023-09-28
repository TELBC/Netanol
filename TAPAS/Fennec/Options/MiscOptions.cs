namespace Fennec.Options;

/// <summary>
/// Configurations only applied at startup.
/// </summary>
public class StartupOptions
{
    /// <summary>
    /// Should the swagger ui be reachable with /swagger.
    /// </summary>
    public bool EnableSwagger { get; set; }
}