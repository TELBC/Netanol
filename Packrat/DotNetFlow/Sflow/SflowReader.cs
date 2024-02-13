using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using PcapDotNet;
using PcapDotNet.Base;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;

namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Reads sFlow datagrams
    /// </summary>
    public class SflowReader : ISflowReader
    {
        public Header ReadHeader(Stream stream)
        {
            // dangerous, reader not disposed
            var reader = new BinaryReader(stream);

            var header = new Header
            {
                Version = reader.ReadUInt32().ToNetworkByteOrder(),
                AgentAddressType = reader.ReadUInt32().ToNetworkByteOrder()
            };

            header.AgentAddress = header.AgentAddressType switch
            {
                1 => new IPAddress(reader.ReadBytes(4)), // IPv4
                2 => new IPAddress(reader.ReadBytes(16)), // IPv6
                _ => header.AgentAddress
            };

            header.SubAgentId = reader.ReadUInt32().ToNetworkByteOrder();
            header.SequenceNumber = reader.ReadUInt32().ToNetworkByteOrder();
            header.SysUpTime = reader.ReadUInt32().ToNetworkByteOrder();
            header.NumSamples = reader.ReadUInt32().ToNetworkByteOrder();

            return header;
        }

        public IEnumerable<ISample> ReadSamples(Stream datagramStream, uint numSamples)
        {
            using var reader = new BinaryReader(datagramStream);
            var samples = new List<ISample>();
            for (var i = 0; i < numSamples; i++)
            {
                var sample = ReadSample(reader);
                samples.Add(sample);
            }

            return samples;
        }

        private ISample ReadSample(BinaryReader reader)
        {
            var enterprise = (Enterprise)reader.ReadUInt16().ToNetworkByteOrder();
            var sampleType = (SampleType)reader.ReadUInt16().ToNetworkByteOrder();

            if (Enterprise.StandardSflow == enterprise) // enterprise == 0
            {
                if (sampleType == SampleType.CounterSample)
                {
                    var counterSample = new CounterSample
                    {
                        Enterprise = enterprise,
                        Type = sampleType,
                        SampleLength = reader.ReadUInt32().ToNetworkByteOrder(),
                        SequenceNumber = reader.ReadUInt32().ToNetworkByteOrder(),
                        SourceIdType = reader.ReadUInt16().ToNetworkByteOrder(),
                        SourceIdIndex = reader.ReadUInt16().ToNetworkByteOrder(),
                        CounterType = (CounterFormat)reader.ReadUInt32().ToNetworkByteOrder()
                    };

                    if (counterSample.CounterType == CounterFormat.GenericInterfaceCounters)
                    {
                        counterSample.CounterRecord = new GenericInterfaceCounters
                        {
                            Enterprise = (Enterprise)reader.ReadUInt16().ToNetworkByteOrder(),
                            Format = (CounterFormat)reader.ReadUInt16().ToNetworkByteOrder(),
                            FlowDataLength = reader.ReadUInt32().ToNetworkByteOrder(),
                            IfIndex = reader.ReadUInt32().ToNetworkByteOrder(),
                            IfType = reader.ReadUInt32().ToNetworkByteOrder(),
                            IfSpeed = reader.ReadUInt32().ToNetworkByteOrder(),
                            IfDirection = (IfDirection)reader.ReadUInt32().ToNetworkByteOrder(),
                            IfAdminStatus = reader.ReadUInt16().ToNetworkByteOrder(),
                            IfOperStatus = reader.ReadUInt16().ToNetworkByteOrder(),
                            InputOctets = reader.ReadUInt64().ToNetworkByteOrder(),
                            InputPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            InputMulticastPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            InputBroadcastPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            InputDiscardedPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            InputErrors = reader.ReadUInt32().ToNetworkByteOrder(),
                            InputUnknownProtocolPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            OutputOctets = reader.ReadUInt64().ToNetworkByteOrder(),
                            OutputPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            OutputMulticastPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            OutputBroadcastPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            OutputDiscardedPackets = reader.ReadUInt32().ToNetworkByteOrder(),
                            OutputErrors = reader.ReadUInt32().ToNetworkByteOrder(),
                            PromiscuousMode = reader.ReadUInt32().ToNetworkByteOrder()
                        };
                    }

                    return counterSample;
                }
                else if (sampleType == SampleType.FlowSample)
                {
                    var flowSample = new FlowSample
                    {
                        Enterprise = enterprise,
                        Type = sampleType,
                        SampleLength = reader.ReadUInt32().ToNetworkByteOrder(),
                        SequenceNumber = reader.ReadUInt32().ToNetworkByteOrder(),
                        SourceIdClass = reader.ReadUInt16().ToNetworkByteOrder(),
                        SourceIdIndex = reader.ReadUInt16().ToNetworkByteOrder(),
                        SamplingRate = reader.ReadUInt32().ToNetworkByteOrder(),
                        SamplePool = reader.ReadUInt32().ToNetworkByteOrder(),
                        Drops = reader.ReadUInt32().ToNetworkByteOrder(),
                        InputInterface = reader.ReadUInt32().ToNetworkByteOrder(),
                        OutputInterface = new OutputInterface
                        {
                            OutputInterfaceFormat = reader.ReadUInt16().ToNetworkByteOrder(),
                            OutputInterfaceValue = reader.ReadUInt16().ToNetworkByteOrder()
                        },
                        FlowFormat = (FlowFormat)reader.ReadUInt32().ToNetworkByteOrder()
                    };

                    if (flowSample.FlowFormat == FlowFormat.RawPacketHeader)
                    {
                        flowSample.FlowRecord = new RawPacketHeader
                        {
                            Enterprise = (Enterprise)reader.ReadUInt16().ToNetworkByteOrder(),
                            Format = (FlowFormat)reader.ReadUInt16().ToNetworkByteOrder(),
                            FlowDataLength = reader.ReadUInt32().ToNetworkByteOrder(),
                            HeaderProtocol = (HeaderProtocol)reader.ReadUInt32().ToNetworkByteOrder(),
                            FrameLength = reader.ReadUInt32().ToNetworkByteOrder(),
                            StrippedBytes = reader.ReadUInt32().ToNetworkByteOrder(),
                            SampledHeaderLength = reader.ReadUInt32().ToNetworkByteOrder()
                        };

                        if (flowSample.FlowRecord is RawPacketHeader rawPacketHeader)
                        {
                            var headerLength = (int)rawPacketHeader.SampledHeaderLength;

                            var headerBytes = reader.ReadBytes(headerLength).BytesSequenceToHexadecimalString();

                            if (rawPacketHeader.HeaderProtocol == HeaderProtocol.Ethernet)
                            {
                                var packet = Packet.FromHexadecimalString(headerBytes, DateTime.UtcNow,
                                    DataLinkKind.Ethernet);
                                rawPacketHeader.Packet = packet;
                            }
                        }

                        return flowSample;
                    }

                    return null;
                }

                return null;
            }

            return null;
        }
    }
}