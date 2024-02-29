using PcapDotNet.Packets;

namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Represents the sFlow Raw Packet Header record.
    /// </summary>
    public class RawPacketHeader : FlowRecord
    {
        public Enterprise Enterprise { get; set; }
        public FlowFormat Format { get; set; }
        public uint FlowDataLength { get; set; }
        public HeaderProtocol HeaderProtocol { get; set; }
        public uint FrameLength { get; set; }
        public uint StrippedBytes { get; set; }
        public uint SampledHeaderLength { get; set; }
        public Packet Packet { get; set; }
    }
}