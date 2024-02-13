using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using PcapDotNet.Packets;

namespace DotNetFlow.Sflow
{
    public class Datagram
    {
        public Header Header;
        List<ISample> Samples;
    }

    public class Header
    {
        public uint Version; // sFlow version
        public uint AgentAddressType; // 1 for IPv4, 2 for IPv6
        public IPAddress AgentAddress; // The address of the sFlow agent
        public uint SubAgentId;
        public uint SequenceNumber;
        public uint SysUpTime; // System uptime in ms
        public uint NumSamples; // Number of samples in the datagram
    }

    public interface IRecord
    {
        Enterprise Enterprise { get; set; }
    }

    public interface ISample
    {
        Enterprise Enterprise { get; set; }
        SampleType Type { get; set; }
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
        public uint InputInterface { get; set; }
        public OutputInterface OutputInterface { get; set; }
        public FlowFormat FlowFormat { get; set; }
        public FlowRecord FlowRecord { get; set; }
    }
    public class OutputInterface
    {
        public ushort OutputInterfaceFormat { get; set; }
        public ushort OutputInterfaceValue { get; set; }
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

    public enum SampleType
    {
        FlowSample = 1,
        CounterSample = 2,
        ExpandedFlowSample = 3,
        ExpandedCounterSample = 4
    }

    public enum Enterprise
    {
        StandardSflow = 0
        // Add more enterprise numbers here
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
    
    public enum IfDirection
    {
        Unknown = 0,
        FullDuplex = 1,
        HalfDuplex = 2,
        In = 3,
        Out = 4
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

    public interface ISflowReader
    {
        // Datagram BuildDatagram(Stream datagramStream); // not sure if this is necessary
        Header ReadHeader(Stream datagramStream);
        IEnumerable<ISample> ReadSamples(Stream datagramStream, uint numSamples);
    }
}