using System.Net;

namespace Fennec.Processing.Graph;

public class TraceNode
{
    public TraceNode(IPAddress address, string name, string? dnsName = null)
    {
        Address = address;
        Name = name;
        DnsName = dnsName;
    }

    public IPAddress Address { get; set; }
    public List<string>? Tags { get; set; }
    public string? DnsName { get; set; }
    public string Name { get; set; }
    public string? HexColor { get; set; }

    /// <summary>
    ///     Create a new key for this node.
    /// </summary>
    public TraceNodeKey Key => new(Address);
}

public record TraceNodeDto(string Id, string Name, string? DnsName, string? HexColor, List<string>? Tags);