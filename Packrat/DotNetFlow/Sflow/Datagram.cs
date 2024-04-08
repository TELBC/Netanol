using System.Collections.Generic;

namespace DotNetFlow.Sflow
{
    /// <summary>
    /// Represents an sFlow datagram.
    /// </summary>
    public class Datagram
    {
        public Header Header;
        List<ISample> Samples;
    }
}