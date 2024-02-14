using System.Net;
using Fennec.Database.Domain;

namespace Fennec.Utils;

// TODO: rename this to reflect its purpose better
public class IpAddressPairComparer : IEqualityComparer<(IPAddress, IPAddress, ushort, ushort, DataProtocol)>,
    IComparer<(IPAddress, IPAddress, ushort, ushort, DataProtocol)>
{
    public int Compare((IPAddress, IPAddress, ushort, ushort, DataProtocol) x,
        (IPAddress, IPAddress, ushort, ushort, DataProtocol) y)
    {
        var result = CompareIPAddresses(x.Item1, y.Item1);
        if (result != 0)
            return result;

        result = CompareIPAddresses(x.Item2, y.Item2);
        if (result != 0)
            return result;

        result = x.Item3.CompareTo(y.Item3);
        if (result != 0)
            return result;

        result = x.Item4.CompareTo(y.Item4);
        if (result != 0)
            return result;

        return x.Item5.CompareTo(y.Item5);
    }

    public bool Equals((IPAddress, IPAddress, ushort, ushort, DataProtocol) x,
        (IPAddress, IPAddress, ushort, ushort, DataProtocol) y)
    {
        // Check if both IP addresses, ports, and the DataProtocol in the tuples are equal
        return x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2) && x.Item3 == y.Item3 && x.Item4 == y.Item4 &&
               x.Item5 == y.Item5;
    }

    public int GetHashCode((IPAddress, IPAddress, ushort, ushort, DataProtocol) obj)
    {
        unchecked // Allow arithmetic overflow, numbers will just "wrap around"
        {
            var hash = 17;
            hash = hash * 23 + obj.Item1.GetHashCode();
            hash = hash * 23 + obj.Item2.GetHashCode();
            hash = hash * 23 + obj.Item3.GetHashCode();
            hash = hash * 23 + obj.Item4.GetHashCode();
            hash = hash * 31 + obj.Item5.GetHashCode(); // Using a different multiplier for the last item
            return hash;
        }
    }

    private int CompareIPAddresses(IPAddress a, IPAddress b)
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