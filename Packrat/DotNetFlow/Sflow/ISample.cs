namespace DotNetFlow.Sflow
{
    public interface ISample
    {
        Enterprise Enterprise { get; set; }
        SampleType Type { get; set; }
    }
    
    public enum SampleType
    {
        FlowSample = 1,
        CounterSample = 2,
        ExpandedFlowSample = 3,
        ExpandedCounterSample = 4
    }
    
    public enum FlowFormat
    {
        RawPacketHeader = 1,
        EthernetFrame = 2,
        IPv4Header = 3,
        IPv6Header = 4,
        ExtendedSwitchData = 1001,
        ExtendedRouterData = 1002,
        ExtendedGatewayData = 1003,
        ExtendedUserFlowData = 1004
    }
    
    public enum CounterFormat
    {
        GenericInterfaceCounters = 1,
        EthernetInterfaceCounters = 2,
        VlanCounters = 5,
        InfiniBandCounters = 9
    }
    
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
        public FlowFormat FlowFormat { get; set; }
        public FlowRecord FlowRecord { get; set; }
    }
    
    public class InterfaceInfo
    {
        public InterfaceFormat InterfaceFormat { get; set; }
        public uint InterfaceValue { get; set; }
    }

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