namespace Fennec.Options;

/// <summary>
/// Options for all of the DNS services.
/// </summary>
public class DnsCheckServiceOptions
{
    /// <summary>
    /// The interval at which the DNS cache is checked for stale entries.
    /// </summary>
    public TimeSpan CheckInterval { get; set; } = TimeSpan.Parse("01:00:00:00"); // format: dd:hh:mm:ss
}