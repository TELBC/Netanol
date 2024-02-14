using System.Net;

namespace Fennec.Utils; 

public class IpAddressComparer : IComparer<IPAddress>
{
    public int Compare(IPAddress? x, IPAddress? y)
    {
        var bytesX = x!.GetAddressBytes();
        var bytesY = y!.GetAddressBytes();

        var lengthComparison = bytesX.Length.CompareTo(bytesY.Length);
        if (lengthComparison != 0) return lengthComparison;

        for (var i = 0; i < bytesX.Length; i++)
        {
            var byteComparison = bytesX[i].CompareTo(bytesY[i]);
            if (byteComparison != 0) return byteComparison;
        }

        return 0;
    }
}