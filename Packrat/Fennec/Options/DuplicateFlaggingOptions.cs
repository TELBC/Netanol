namespace Fennec.Options;

public class DuplicateFlaggingOptions
{
    /// <summary>
    /// How long an exporter has a claim to being the origin.
    /// </summary>
    public TimeSpan ClaimExpirationLifespan { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// The interval between cleaning up unused entries.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(1);
}