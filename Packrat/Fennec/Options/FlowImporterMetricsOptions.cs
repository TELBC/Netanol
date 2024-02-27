namespace Fennec.Options;

/// <summary>
/// Options for the FlowImportMetrics.
/// </summary>
public class FlowImporterMetricsOptions
{
    /// <summary>
    /// Defines how often the flow should be saved into a combined flow.
    /// </summary>
    public TimeSpan TraceSummationPeriod { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Defines for what period the flowImports should be saved.
    /// </summary>
    public TimeSpan FlowSavePeriod { get; set; } = TimeSpan.FromHours(12);
}