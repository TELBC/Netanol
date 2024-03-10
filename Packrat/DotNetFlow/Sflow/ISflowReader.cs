using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using PcapDotNet.Packets;

namespace DotNetFlow.Sflow
{
    public interface ISflowReader
    {
        // Datagram BuildDatagram(Stream datagramStream); // not sure if this is necessary
        Header ReadHeader(Stream datagramStream);
        IEnumerable<ISample> ReadSamples(Stream datagramStream, uint numSamples);
    }
}