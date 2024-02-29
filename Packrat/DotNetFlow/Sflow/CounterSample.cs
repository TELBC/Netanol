namespace DotNetFlow.Sflow
{
    /// <summary>
    /// A counter sample containing a set of counter records from a specific data source.
    /// </summary>
    public class CounterSample : ISample
    {
        public Enterprise Enterprise { get; set; }
        public SampleType Type { get; set; }
        public uint SampleLength { get; set; }
        public uint SequenceNumber { get; set; }
        public ushort SourceIdType { get; set; }
        public ushort SourceIdIndex { get; set; }
        public CounterFormat CounterType { get; set; }
        public CounterRecord CounterRecord { get; set; }
    }
}