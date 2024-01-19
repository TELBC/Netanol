namespace Fennec.Options;

/// <summary>
/// Options for the DNS cache.
/// </summary>
public class DnsCacheOptions
{
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);
    public TimeSpan InvalidationDuration { get; set; } = TimeSpan.FromDays(2);
}