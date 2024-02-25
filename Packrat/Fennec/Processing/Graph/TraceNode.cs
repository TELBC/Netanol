using System.Net;

namespace Fennec.Processing.Graph;

public class TraceNode
{
    public IPAddress Address { get; set; }
    public List<string>? Tags { get; set; }
    public string Name { get; set; }

    public TraceNode(IPAddress address, string name)
    {
        Address = address;
        Name = name;
    }
}

public record TraceNodeDto(string Id, string Name, List<string>? Tags);
