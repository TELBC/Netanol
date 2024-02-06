using System.IO;
using System.Net;

namespace DotNetFlow.Sflow
{
    public class SflowReader : ISflowReader
    {
        public SflowHeader ReadHeader(byte[] datagram)
        {
            using var stream = new MemoryStream(datagram);
            using var reader = new BinaryReader(stream);

            var header = new SflowHeader
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

        public SflowSample ReadSample(byte[] datagram)
        {
            throw new System.NotImplementedException();
        }
    }
}