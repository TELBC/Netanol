using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Fennec.Database.Domain.Technical;

/// <summary>
/// Collection of DNS lookup information for a <see cref="NetworkHost"/>.
/// </summary>
[Owned]
public class DnsInformation
{
    /// <summary>
    /// The <see cref="DnsName"/> that results when the <see cref="NetworkHost.IpAddress"/> was reversed looked up.
    /// </summary>
    [MaxLength(250)]
    public string DnsName { get; set; }
    
    /// <summary>
    /// The last time the <see cref="DnsName"/> has been looked up.
    /// </summary>
    // TODO: add timed relations if the dns name changes
    // Should create a new NetworkHost in that scenario?
    public DateTimeOffset LastAccessedDnsName { get; set; }

    /// <summary>
    /// The <see cref="NetworkDevice"/> this <see cref="NetworkHost"/> belongs to.
    /// </summary>
    public NetworkDevice NetworkDevice { get; set; }
    public long NetworkDeviceId { get; set; }

    public DnsInformation(string dnsName, DateTimeOffset lastAccessedDnsName, NetworkDevice networkDevice)
    {
        DnsName = dnsName;
        LastAccessedDnsName = lastAccessedDnsName;
        NetworkDevice = networkDevice;
    }
    
    public DnsInformation(string dnsName, DateTimeOffset lastAccessedDnsName, NetworkDevice networkDevice, long networkDeviceId)
    {
        DnsName = dnsName;
        LastAccessedDnsName = lastAccessedDnsName;
        NetworkDevice = networkDevice;
        NetworkDeviceId = networkDeviceId;
    }
    

#pragma warning disable CS8618
    public DnsInformation(string dnsName, DateTimeOffset lastAccessedDnsName, long networkDeviceId)
#pragma warning restore CS8618
    {
        DnsName = dnsName;
        LastAccessedDnsName = lastAccessedDnsName;
        NetworkDeviceId = networkDeviceId;
    }

#pragma warning disable CS8618
    public DnsInformation() { }
#pragma warning restore CS8618
}

/// <summary>
/// Represents an <see cref="IpAddress"/> matched to a <see cref="DnsName"/>.
/// </summary>
public class NetworkHost
{
    public long Id { get; set; }
    
    /// <summary>
    /// The <see cref="IpAddress"/> being matched using reverse DNS lookup.
    /// </summary>
    public IPAddress IpAddress { get; set; }
    
    /// <summary>
    /// Various information about the DNS lookup for the <see cref="IpAddress"/>. This is set once a reverse lookup
    /// has been completed.
    /// </summary>
    public DnsInformation? DnsInformation { get; set; }

    public NetworkHost (IPAddress ipAddress, NetworkDevice networkDevice)
    {
        IpAddress = ipAddress;
        DnsInformation = new DnsInformation(networkDevice.DnsName, DateTimeOffset.UtcNow, networkDevice, networkDevice.Id);
    }
    public NetworkHost(IPAddress ipAddress)
    {
        IpAddress = ipAddress;
    }

#pragma warning disable CS8618
    public NetworkHost() { }
#pragma warning restore CS8618
}