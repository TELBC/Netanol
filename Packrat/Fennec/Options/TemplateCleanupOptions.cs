using Microsoft.Extensions.Options;

namespace Fennec.Options;

/// <summary>
/// Options for cleaning up IPFIX and NetFlow9 templates.
/// </summary>
public class TemplateCleanupOptions
{
    public TimeSpan IpFixCleanupInterval { get; set; } = TimeSpan.FromDays(2);
    public TimeSpan NetFlow9CleanupInterval { get; set; } = TimeSpan.FromDays(2);
}