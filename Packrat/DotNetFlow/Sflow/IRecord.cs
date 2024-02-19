using PcapDotNet.Packets;

namespace DotNetFlow.Sflow
{
    public interface IRecord
    {
        Enterprise Enterprise { get; set; }
    }
    
    public abstract class FlowRecord : IRecord
    {
        public Enterprise Enterprise { get; set; }
        public FlowFormat Format;
    }

    public abstract class CounterRecord : IRecord
    {
        public Enterprise Enterprise { get; set; }
        public CounterFormat Format;
    }
    
    public class GenericInterfaceCounters : CounterRecord
    {
        public Enterprise Enterprise { get; set; }
        public CounterFormat Format { get; set; } // 2 bytes
        public uint FlowDataLength { get; set; } // 2 bytes
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
    
    public class RawPacketHeader : FlowRecord
    {
        public Enterprise Enterprise { get; set; } // 2 bytes
        public FlowFormat Format { get; set; } // 2 bytes
        public uint FlowDataLength { get; set; } // 4 bytes
        public HeaderProtocol HeaderProtocol { get; set; } // 4 bytes
        public uint FrameLength { get; set; } // 4 bytes
        public uint StrippedBytes { get; set; } // 4 bytes
        public uint SampledHeaderLength { get; set; } // 4 bytes
        public Packet Packet { get; set; }
    }
    
    public enum HeaderProtocol
    {
        Ethernet = 1,
        Ppp = 7,
        Ipv4 = 11,
        Ipv6 = 12,
        Mpls = 13,
    }
}