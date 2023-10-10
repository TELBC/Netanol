using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database.Domain.Technical;

[Owned]
public record DnsInfo(string DnsName, DateTimeOffset LastChecked);

/// <summary>
/// Represents an <see cref="IpAddress"/> matched to a <see cref="DnsName"/>.
/// </summary>
public class NetworkHost
{
    public NetworkHost(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
    }

#pragma warning disable CS8618
    public NetworkHost()
    {
    }
#pragma warning restore CS8618
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="IpAddress"/> being matched using reverse DNS lookup.
    /// </summary>
    public IPAddress IpAddress { get; set; }

    /// <summary>
    /// Various information about the DNS lookup for the <see cref="IpAddress"/>. This is set once a reverse lookup
    /// has been completed.
    /// </summary>
    public DnsInfo? DnsInfo { get; set; }
}