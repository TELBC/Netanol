using System.ComponentModel.DataAnnotations;

namespace Tapas.Models;

/// <summary>
/// A single device on the network uniquely identified by its DNS entry.
/// </summary>
public class NetworkDevice
{
    public long Id { get; set; }
    
    /// <summary>
    /// The DNS name of this <see cref="NetworkDevice"/>.
    /// </summary>
    [MaxLength(250)]
    public string DnsName { get; set; }

    /// <summary>
    /// All <see cref="NetworkHosts"/> that resolve to the <see cref="DnsName"/> of this <see cref="NetworkDevice"/>.
    /// </summary>
    public ICollection<NetworkHost> NetworkHosts { get; set; } = default!;
    
    /// <summary>
    /// All <see cref="GraphNodes"/> this <see cref="NetworkDevice"/> is a part of.
    /// </summary>
    public ICollection<GraphNode> GraphNodes { get; set; } = default!;

    public NetworkDevice(string dnsName)
    {
        DnsName = dnsName;
    }

#pragma warning disable CS8618
    public NetworkDevice() { }
#pragma warning restore CS8618
}