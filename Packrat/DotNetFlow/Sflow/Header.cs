using System.Net;

namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Represents the sFlow header.
    /// </summary>
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
}