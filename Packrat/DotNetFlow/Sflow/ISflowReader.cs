using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using PcapDotNet.Packets;

namespace DotNetFlow.Sflow
{
    public enum InterfaceFormat
    {
        SingleInterface =  0,
        PacketDiscarded =  1,
        MultipleDestinationInterfaces =  2
    }

    public enum IfDirection
    {
        Unknown = 0,
        FullDuplex = 1,
        HalfDuplex = 2,
        In = 3,
        Out = 4
    }

    public interface ISflowReader
    {
        // Datagram BuildDatagram(Stream datagramStream); // not sure if this is necessary
        Header ReadHeader(Stream datagramStream);
        IEnumerable<ISample> ReadSamples(Stream datagramStream, uint numSamples);
    }
}