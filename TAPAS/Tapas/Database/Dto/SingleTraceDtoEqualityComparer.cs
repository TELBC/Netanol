using System.Net;

namespace Tapas.Database.Dto;

public class SingleTraceDtoEqualityComparer : IEqualityComparer<SingleTraceDto>
{
    public bool Equals(SingleTraceDto x, SingleTraceDto y)
    {
        if (ReferenceEquals(x, y))
            return true;

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            return false;

        return x.Protocol == y.Protocol &&
               IPAddress.Equals(x.SourceIpAddress, y.SourceIpAddress) &&
               x.SourcePort == y.SourcePort &&
               IPAddress.Equals(x.DestinationIpAddress, y.DestinationIpAddress) &&
               x.DestinationPort == y.DestinationPort;
    }
    
    public int GetHashCode(SingleTraceDto obj)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + obj.Protocol.GetHashCode();
            hash = hash * 23 + (obj.SourceIpAddress?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.SourcePort.GetHashCode();
            hash = hash * 23 + (obj.DestinationIpAddress?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.DestinationPort.GetHashCode();
            return hash;
        }
    }
}