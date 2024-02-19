using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
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
        /// <summary>
        /// Reads the header of the sFlow datagram
        /// </summary>
        /// <param name="datagramStream"></param>
        /// <returns></returns>
        public Header ReadHeader(Stream datagramStream)
        {
            using var reader = new BinaryReader(datagramStream, Encoding.UTF8, true); 

            var header = new Header
            {
                Version = reader.ReadUInt32().ToNetworkByteOrder(),
                AgentAddressType = reader.ReadUInt32().ToNetworkByteOrder(),
            };
            header.AgentAddress = GetAgentAddress(header.AgentAddressType, reader);
            header.SubAgentId = reader.ReadUInt32().ToNetworkByteOrder();
            header.SequenceNumber = reader.ReadUInt32().ToNetworkByteOrder();
            header.SysUpTime = reader.ReadUInt32().ToNetworkByteOrder();
            header.NumSamples = reader.ReadUInt32().ToNetworkByteOrder();

            return header;
        }

        /// <summary>
        /// Reads multiple sFlow samples
        /// </summary>
        /// <param name="datagramStream"></param>
        /// <param name="numSamples"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Reads the sFlow sample
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ISample ReadSample(BinaryReader reader)
        {
            var enterprise = (Enterprise)reader.ReadUInt16().ToNetworkByteOrder();
            var sampleType = (SampleType)reader.ReadUInt16().ToNetworkByteOrder();

            if (Enterprise.StandardSflow == enterprise) // enterprise == 0
            {
                switch (sampleType)
                {
                    case SampleType.CounterSample:
                    {
                        var counterSample = new CounterSample
                        {
                            Enterprise = enterprise,
                            Type = sampleType,
                        };

                        return ReadCounterSample(reader, counterSample);
                    }
                    case SampleType.FlowSample:
                    {
                        var flowSample = new FlowSample
                        {
                            Enterprise = enterprise,
                            Type = sampleType
                        };

                        return ReadFlowSample(reader, flowSample);
                    }
                    case SampleType.ExpandedFlowSample: // add exception (not implemented)
                    case SampleType.ExpandedCounterSample: // add exception (not implemented)
                    default:
                        return null; // add exception
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the agent address based on the agent address type.
        /// </summary>
        /// <param name="agentAddressType"></param>
        /// <param name="reader"></param>
        /// <returns></returns>
        private IPAddress GetAgentAddress(uint agentAddressType, BinaryReader reader)
        {
            return agentAddressType switch
            {
                1 => new IPAddress(reader.ReadBytes(4)), // IPv4
                2 => new IPAddress(reader.ReadBytes(16)), // IPv6
                _ => null
            };
        }

        /// <summary>
        /// Reads the counter sample.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="counterSample"></param>
        /// <returns></returns>
        private CounterSample ReadCounterSample(BinaryReader reader, CounterSample counterSample)
        {
            counterSample.SampleLength = reader.ReadUInt32().ToNetworkByteOrder();
            counterSample.SequenceNumber = reader.ReadUInt32().ToNetworkByteOrder();
            counterSample.SourceIdType = reader.ReadUInt16().ToNetworkByteOrder();
            counterSample.SourceIdIndex = reader.ReadUInt16().ToNetworkByteOrder();
            counterSample.CounterType = (CounterFormat)reader.ReadUInt32().ToNetworkByteOrder();

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

        /// <summary>
        /// Reads the flow sample.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="flowSample"></param>
        /// <returns></returns>
        private FlowSample ReadFlowSample(BinaryReader reader, FlowSample flowSample)
        {
            flowSample.SampleLength = reader.ReadUInt32().ToNetworkByteOrder();
            flowSample.SequenceNumber = reader.ReadUInt32().ToNetworkByteOrder();
            flowSample.SourceIdClass = reader.ReadUInt16().ToNetworkByteOrder();
            flowSample.SourceIdIndex = reader.ReadUInt16().ToNetworkByteOrder();
            flowSample.SamplingRate = reader.ReadUInt32().ToNetworkByteOrder();
            flowSample.SamplePool = reader.ReadUInt32().ToNetworkByteOrder();
            flowSample.Drops = reader.ReadUInt32().ToNetworkByteOrder();
            flowSample.InputInterface = ParseInputInterface(reader);
            flowSample.OutputInterface = ParseOutputInterface(reader);
            flowSample.FlowFormat = (FlowFormat)reader.ReadUInt32().ToNetworkByteOrder();

            if (flowSample.FlowFormat == FlowFormat.RawPacketHeader)
            {
                ReadRawPacketHeader(reader, flowSample);

                return flowSample;
            }

            return flowSample;
        }

        /// <summary>
        /// Reads the raw packet header.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="flowSample"></param>
        private void ReadRawPacketHeader(BinaryReader reader, FlowSample flowSample)
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
        }
        
        /// <summary>
        /// Parses the input interface
        /// <see cref="InterfaceFormat"/> values 1 & 2 only apply to output interfaces!
        /// </summary>
        /// <param name="reader"></param>
        /// <returns><see cref="InterfaceInfo"/> object containing the format (type) of the interface and the interface value</returns>
        private InterfaceInfo ParseInputInterface(BinaryReader reader)
        {
            var interfaceValue = reader.ReadUInt32().ToNetworkByteOrder();
            var format = (InterfaceFormat)(interfaceValue >> 30); // Extract the  2 most significant bits
            var value = interfaceValue & 0x3FFFFFFF; // Mask out the format bits

            if (format == InterfaceFormat.PacketDiscarded || format == InterfaceFormat.MultipleDestinationInterfaces)
            {
                throw new InvalidDataException(
                    "Invalid format for input interface. Format  1 and  2 are only valid for output interfaces.");
            }

            return new InterfaceInfo()
            {
                InterfaceFormat = format,
                InterfaceValue = value
            };
        }
        
        /// <summary>
        /// Parses the output interface
        /// <see cref="InterfaceFormat"/> values 1 & 2 only apply to output interfaces!
        /// </summary>
        /// <param name="reader"></param>
        /// <returns><see cref="InterfaceInfo"/> object containing the format (type) of the interface and the interface value</returns>
        private InterfaceInfo ParseOutputInterface(BinaryReader reader)
        {
            var interfaceValue = reader.ReadUInt32().ToNetworkByteOrder();
            var format = (InterfaceFormat)(interfaceValue >> 30); // Extract the  2 most significant bits
            var value = interfaceValue & 0x3FFFFFFF; // Mask out the format bits

            return new InterfaceInfo()
            {
                InterfaceFormat = format,
                InterfaceValue = value
            };
        }
    }
}