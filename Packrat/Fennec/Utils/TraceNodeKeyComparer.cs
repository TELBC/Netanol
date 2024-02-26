using System.Net;
using Fennec.Processing.Graph;

namespace Fennec.Utils; 

public class TraceNodeKeyComparer : IComparer<TraceNodeKey>
{
    public int Compare(TraceNodeKey? x, TraceNodeKey? y)
    {
        if (x == y) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        
        var bytesX = x.Address.GetAddressBytes();
        var bytesY = y.Address.GetAddressBytes();

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