using System.Collections.Generic;

namespace DotNetFlow.Sflow
{
    public class Datagram
    {
        public Header Header;
        List<ISample> Samples;
    }
}