using System.Net;
using System.Net.Sockets;
using DotNetFlow.Sflow;
using Fennec.Database;
using Fennec.Database.Domain;
using PcapDotNet.Packets.Ethernet;

namespace Fennec.Parsers;

public class SflowParser : IParser
{
    private readonly ILogger _log;

    public SflowParser(ILogger log)
    {
        _log = log.ForContext<SflowParser>();
    }

    public IEnumerable<TraceImportInfo> Parse(UdpReceiveResult result)
    {
        using var stream = new MemoryStream(result.Buffer);
        var reader = new SflowReader();

        var header = reader.ReadHeader(stream);
        var samples = reader.ReadSamples(stream, header.NumSamples);
        var traceImportInfos = new List<TraceImportInfo>();

        foreach (var sample in samples)
        {
            switch (sample)
            {
                case CounterSample counterSample:
                    _log.Verbose(
                        "Received counter sample from {AgentAddress}, dropping due to lack of relevance for topology visualization.",
                        header.AgentAddress);
                    break;
                case FlowSample flowSample:
                {
                    foreach (var flowRecord in flowSample.FlowRecords)
                    {
                        if (flowRecord is RawPacketHeader rawPacketHeader)
                        {
                            if (rawPacketHeader.HeaderProtocol == HeaderProtocol.Ethernet &&
                                rawPacketHeader.Packet.Ethernet.IsValid)
                            {
                                if (rawPacketHeader.Packet.Ethernet.EtherType == EthernetType.VLanTaggedFrame)
                                {
                                    var traceImportInfo = new TraceImportInfo
                                    (
                                        DateTime.UtcNow,
                                        result.RemoteEndPoint.Address,
                                        IPAddress.Parse(
                                            rawPacketHeader.Packet.Ethernet.VLanTaggedFrame.IpV4.Source.ToString()),
                                        rawPacketHeader.Packet.Ethernet.VLanTaggedFrame.IpV4.Tcp.SourcePort,
                                        IPAddress.Parse(rawPacketHeader.Packet.Ethernet.VLanTaggedFrame.IpV4.Destination
                                            .ToString()),
                                        rawPacketHeader.Packet.Ethernet.VLanTaggedFrame.IpV4.Tcp.DestinationPort,
                                        1, // FlowSample provides us with a single packet (sample) does not represent the number of packets transmitted,
                                        (ulong)rawPacketHeader.Packet.Ethernet.VLanTaggedFrame.IpV4.TotalLength,
                                        MapToDataProtocol((ushort)rawPacketHeader.Packet.Ethernet.VLanTaggedFrame.IpV4
                                            .Protocol), // fix casting
                                        FlowProtocol.Sflow
                                    );
                                    traceImportInfos.Add(traceImportInfo);
                                }
                                else if (rawPacketHeader.Packet.Ethernet.EtherType == EthernetType.IpV4 &&
                                         rawPacketHeader.Packet.Ethernet.IpV4.IsValid)
                                {
                                    var traceImportInfo = new TraceImportInfo
                                    (
                                        DateTime.UtcNow,
                                        result.RemoteEndPoint.Address,
                                        IPAddress.Parse(rawPacketHeader.Packet.Ethernet.IpV4.Source.ToString()),
                                        rawPacketHeader.Packet.Ethernet.IpV4.Tcp.SourcePort,
                                        IPAddress.Parse(rawPacketHeader.Packet.Ethernet.IpV4.Destination.ToString()),
                                        rawPacketHeader.Packet.Ethernet.IpV4.Tcp.DestinationPort,
                                        1, // FlowSample provides us with a single packet (sample) does not represent the number of packets transmitted,
                                        (ulong)rawPacketHeader.Packet.Ethernet.IpV4.TotalLength,
                                        MapToDataProtocol((ushort)rawPacketHeader.Packet.Ethernet.IpV4
                                            .Protocol), // fix casting
                                        FlowProtocol.Sflow
                                    );
                                    traceImportInfos.Add(traceImportInfo);
                                }
                                else if (rawPacketHeader.Packet.Ethernet.EtherType == EthernetType.IpV6 &&
                                         rawPacketHeader.Packet.Ethernet.IpV6.IsValid)
                                {
                                    var traceImportInfo = new TraceImportInfo
                                    (
                                        DateTime.UtcNow,
                                        result.RemoteEndPoint.Address,
                                        IPAddress.Parse(rawPacketHeader.Packet.Ethernet.IpV6.Source.ToString()),
                                        rawPacketHeader.Packet.Ethernet.IpV6.Tcp.SourcePort,
                                        IPAddress.Parse(rawPacketHeader.Packet.Ethernet.IpV6.CurrentDestination
                                            .ToString()), // Destination does not exist in PcapDotNet apparently
                                        rawPacketHeader.Packet.Ethernet.IpV6.Tcp.DestinationPort,
                                        1, // FlowSample provides us with a single packet (sample) does not represent the number of packets transmitted,
                                        (ulong)rawPacketHeader.Packet.Ethernet.IpV6.TotalLength,
                                        MapToDataProtocol((ushort)rawPacketHeader.Packet.Ethernet.IpV6
                                            .NextHeader), // fix casting
                                        FlowProtocol.Sflow
                                    );
                                    traceImportInfos.Add(traceImportInfo);
                                }
                            }
                            else if (rawPacketHeader.HeaderProtocol == HeaderProtocol.Ipv4)
                            {
                                _log.Verbose(
                                    "Received RawPacketHeader sample starting with Ipv4 Header from {AgentAddress}. Logic Not implemented. Dropping.",
                                    header.AgentAddress);
                            }
                            else if (rawPacketHeader.HeaderProtocol == HeaderProtocol.Ipv6)
                            {
                                _log.Verbose("Received RawPacketHeader sample starting with Ipv6 Header from {AgentAddress}. Logic Not implemented. Dropping.",
                                    header.AgentAddress);
                            }
                        }
                    }
                    
                    break;
                }
            }
        }

        return traceImportInfos;
    }

    private static DataProtocol MapToDataProtocol(ushort protocol)
    {
        return protocol switch
        {
            1 => DataProtocol.Icmp,
            6 => DataProtocol.Tcp,
            17 => DataProtocol.Udp,
            _ => DataProtocol.Unknown
        };
    }
}