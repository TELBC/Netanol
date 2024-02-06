using System.Net;
using System.Runtime.InteropServices;

namespace DotNetFlow.Sflow
{
    public struct SflowHeader
    {
        public uint Version;
        public uint AgentAddressType; // change to correct type
        public IPAddress AgentAddress;
        public uint SubAgentId;
        public uint SequenceNumber;
        public uint SysUpTime;
        public uint NumSamples;
    }
    
    public class SflowSample
    {
    }

    public interface ISflowReader
    {
        SflowHeader ReadHeader(byte[] datagram);
        SflowSample ReadSample(byte[] datagram);
    }
}