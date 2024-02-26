using System.Net;

namespace Fennec.Processing.Graph;

public class TraceNode
{
    public TraceNode(IPAddress address, string name)
    {
        Address = address;
        Name = name;
    }

    public IPAddress Address { get; set; }
    public List<string>? Tags { get; set; }
    public string Name { get; set; }

    /// <summary>
    ///     Create a new key for this node.
    /// </summary>
    public TraceNodeKey Key => new(Address);
}

public record TraceNodeDto(string Id, string Name, List<string>? Tags);