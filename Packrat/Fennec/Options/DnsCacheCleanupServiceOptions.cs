namespace Fennec.Options;

public class DnsCacheCleanupServiceOptions
{
    /// <summary>
    /// The interval at which the DNS cache is cleaned up.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.Parse("00:06:00:00"); // format: dd:hh:mm:ss
}