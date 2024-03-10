using System.Collections.Generic;

namespace DotNetFlow.Sflow
{
    /// <summary>
    /// A flow sample containing a set of flow records from a specific data source.
    /// </summary>
    public class FlowSample : ISample
    {
        public Enterprise Enterprise { get; set; }
        public SampleType Type { get; set; }
        public uint SampleLength { get; set; }
        public uint SequenceNumber { get; set; }
        public ushort SourceIdClass { get; set; }
        public ushort SourceIdIndex { get; set; }
        public uint SamplingRate { get; set; }
        public uint SamplePool { get; set; }
        public uint Drops { get; set; }
        public InterfaceInfo InputInterface { get; set; }
        public InterfaceInfo OutputInterface { get; set; }
        public uint NumRecords { get; set; }
        public List<FlowRecord> FlowRecords { get; set; }
    }
}