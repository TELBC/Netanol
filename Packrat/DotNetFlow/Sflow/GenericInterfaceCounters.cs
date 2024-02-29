namespace DotNetFlow.Sflow
{
    /// <summary>
    ///  Represents the sFlow Generic Interface Counters record.
    /// </summary>
    public class GenericInterfaceCounters : CounterRecord
    {
        public Enterprise Enterprise { get; set; }
        public CounterFormat Format { get; set; }
        public uint FlowDataLength { get; set; }
        public uint IfIndex { get; set; }
        public uint IfType { get; set; }
        public uint IfSpeed { get; set; }
        public IfDirection IfDirection { get; set; }
        public ushort IfAdminStatus { get; set; }
        public ushort IfOperStatus { get; set; }
        public ulong InputOctets { get; set; }
        public uint InputPackets { get; set; }
        public uint InputMulticastPackets { get; set; }
        public uint InputBroadcastPackets { get; set; }
        public uint InputDiscardedPackets { get; set; }
        public uint InputErrors { get; set; }
        public uint InputUnknownProtocolPackets { get; set; }
        public ulong OutputOctets { get; set; }
        public uint OutputPackets { get; set; }
        public uint OutputMulticastPackets { get; set; }
        public uint OutputBroadcastPackets { get; set; }
        public uint OutputDiscardedPackets { get; set; }
        public uint OutputErrors { get; set; }
        public uint PromiscuousMode { get; set; }   
    }
}