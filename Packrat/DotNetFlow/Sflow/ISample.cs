namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Interface for all sample types
    /// </summary>
    public interface ISample
    {
        Enterprise Enterprise { get; set; }
        SampleType Type { get; set; }
    }

    /// <summary>
    /// Enumeration of the different sample types
    /// These are not all sample types, the most common are FlowSample and CounterSample.
    /// </summary>
    public enum SampleType
    {
        FlowSample = 1,
        CounterSample = 2,
        ExpandedFlowSample = 3,
        ExpandedCounterSample = 4
    }
}