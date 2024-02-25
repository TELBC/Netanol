using Fennec.Processing.Graph;
using Fennec.Services;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Processing;

/// <summary>
///     Add tags to the nodes based on the current VMware tags.
/// </summary>
public class VmwareTaggingLayer : ILayer
{
    [BsonElement("type")]
    public string Type { get; set; } = LayerType.VmwareTagging;
    
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("enabled")]
    public bool Enabled { get; set; }
    
    [BsonIgnore]
    public string Description => "";
    
    public void Execute(ITraceGraph graph, IServiceProvider serviceProvider)
    {
        var log = serviceProvider.GetRequiredService<ILogger>().ForContext<VmwareTaggingLayer>();
        var vmwareService = serviceProvider.GetService<ITagsCacheService>();
        
        if (vmwareService is null)
        {
            log.Warning("TagsCacheService not configured... Skipping VMware tagging");
            return;
        }
        
        foreach (var node in graph.Nodes.Values)
        {
            var tags = vmwareService.GetTags(node.Address);
            if (tags is null)
                continue;
            
            node.Tags ??= new List<string>();
            node.Tags.AddRange(tags);
        }
    }
}

public record VmwareTaggingLayerDto(string Type, string? Name, bool Enabled) : ILayerDto;
