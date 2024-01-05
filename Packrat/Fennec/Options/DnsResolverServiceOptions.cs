namespace Fennec.Options;

public class DnsResolverServiceOptions
{
    /// <summary>
    /// The duration after which a DNS entry is considered stale and removed from the cache.
    /// </summary>
    public TimeSpan StaleEntryDuration { get; set; } = TimeSpan.Parse("02:00:00:00"); // format: dd:hh:mm:ss
}