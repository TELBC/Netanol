using System.Net;
using Fennec.Processing.Graph;

namespace Fennec.Utils;

// TODO: rename this to reflect its purpose better
public class TraceEdgeKeyComparer : IEqualityComparer<TraceEdgeKey>, IComparer<TraceEdgeKey>
{
    public int Compare(TraceEdgeKey? x, TraceEdgeKey? y)
    {
        // Handle null cases here
        if (x == y) return 0;
        if (x is null) return -1;
        if (y is null) return 1; 
        
        var result = CompareIpAddresses(x.Source.Address, y.Source.Address);
        if (result != 0)
            return result;

        result = CompareIpAddresses(x.Target.Address, y.Target.Address);
        if (result != 0)
            return result;

        result = x.SourcePort.CompareTo(y.SourcePort);
        if (result != 0)
            return result;

        result = x.TargetPort.CompareTo(y.TargetPort);
        if (result != 0)
            return result;

        return x.DataProtocol.CompareTo(y.DataProtocol);
    }

    public bool Equals(TraceEdgeKey? x, TraceEdgeKey? y)
    {
        // Handle nulls here
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        return x.Equals(y);
    }

    public int GetHashCode(TraceEdgeKey obj) => obj.GetHashCode();

    private static int CompareIpAddresses(IPAddress a, IPAddress b)
    {
        // Convert IP addresses to byte arrays and compare them
        var bytesA = a.GetAddressBytes();
        var bytesB = b.GetAddressBytes();

        // Compare lengths first (IPv6 > IPv4)
        var lengthComparison = bytesA.Length.CompareTo(bytesB.Length);
        if (lengthComparison != 0) return lengthComparison;

        // If lengths are equal, compare byte-by-byte
        for (var i = 0; i < bytesA.Length; i++)
        {
            var byteComparison = bytesA[i].CompareTo(bytesB[i]);
            if (byteComparison != 0) return byteComparison;
        }

        // IP addresses are equal
        return 0;
    }
}