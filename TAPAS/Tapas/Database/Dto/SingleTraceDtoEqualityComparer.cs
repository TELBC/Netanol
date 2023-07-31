using System.Net;

namespace Tapas.Database.Dto;

public class SingleTraceDtoEqualityComparer : IEqualityComparer<SingleTraceDto>
{
    public bool Equals(SingleTraceDto? x, SingleTraceDto? y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            return false;

        return x.SourceIpv6Address != null &&
               y.DestinationIpv4Address != null &&
               x is { DestinationIpv6Address: not null, DestinationIpv4Address: not null } &&
               y is { SourceIpv6Address: not null, SourceIpv4Address: not null } &&
               x.SourceIpv4Address != null &&
               y.DestinationIpv6Address != null &&
               x.Protocol == y.Protocol &&
               IPAddress.Equals(IPAddress.Parse(x.SourceIpv4Address), IPAddress.Parse(y.SourceIpv4Address)) &&
               IPAddress.Equals(IPAddress.Parse(x.SourceIpv6Address), IPAddress.Parse(y.SourceIpv6Address)) &&
               x.SourcePort == y.SourcePort &&
               IPAddress.Equals(IPAddress.Parse(x.DestinationIpv4Address),
                   IPAddress.Parse(y.DestinationIpv4Address)) &&
               IPAddress.Equals(IPAddress.Parse(x.DestinationIpv6Address),
                   IPAddress.Parse(y.DestinationIpv6Address)) &&
               x.DestinationPort == y.DestinationPort;
    }

    public int GetHashCode(SingleTraceDto obj)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + obj.Protocol.GetHashCode();
            hash = hash * 23 + (obj.SourceIpv4Address?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.SourceIpv6Address?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.SourcePort.GetHashCode();
            hash = hash * 23 + (obj.DestinationIpv4Address?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.DestinationIpv6Address?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.DestinationPort.GetHashCode();
            return hash;
        }
    }
}