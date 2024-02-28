namespace Fennec.Options;

/// <summary>
///     Options for the DNS cache.
/// </summary>
public class DnsCacheOptions
{
    /// <summary>
    ///     Whether the DNS cache is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    ///     The interval between cache cleanups.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    ///     How long a DNS entry is valid for.
    /// </summary>
    public TimeSpan InvalidationDuration { get; set; } = TimeSpan.FromDays(2);
}