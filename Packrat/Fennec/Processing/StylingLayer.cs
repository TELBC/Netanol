using Fennec.Database.Domain;
using Fennec.Processing.Graph;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Fennec.Processing;

// TODO: rethink this entire styling layer

public enum ScoringMode
{
    PacketCount,
    ByteCount,
    Calculated
}

public record ColorRange(string StartHex, string EndHex);

/// <summary>
///     Handles the styling of edges in the graph.
/// </summary>
public class EdgeStyler
{
    /*
     * Width can be based on the packet count, byte count, or the multiplication of the two
     */

    [BsonElement("setWidth")]
    public bool SetWidth { get; set; }

    [BsonElement("widthScoringMode")]
    public ScoringMode WidthScoringMode { get; set; }

    [BsonElement("edgeMinWidth")]
    public float EdgeMinWidth { get; set; }

    [BsonElement("edgeMaxWidth")]
    public float EdgeMaxWidth { get; set; }

    /*
     * Color can be interpolated between two colors, or based on the packet count, byte count, or the multiplication of the two
     * Colors do not have to be interpolated
     */

    [BsonElement("setColor")]
    public bool SetColor { get; set; }

    [BsonElement("colorScoringMode")]
    public ScoringMode ColorScoringMode { get; set; }

    [BsonElement("interpolateColors")]
    public bool InterpolateColors { get; set; }

    [BsonElement("useProtocolColors")]
    public bool UseProtocolColors { get; set; }

    [BsonElement("protocolColors")]
    public Dictionary<DataProtocol, ColorRange> ProtocolColors { get; set; } = new();

    /// <summary>
    /// Set the <see cref="TraceEdge.Width"/> and <see cref="TraceEdge.HexColor"/> attributes based on the following rules: <br/>
    /// 1. Return if there are no edges <br/>
    /// 2. Get the scores for all edges based on the <see cref="WidthScoringMode"/> and <see cref="ColorScoringMode"/> <br/>
    /// 3. Get the minimum and maximum scores for both <br/>
    /// 4. Return if the width should be set, but there are no scores or if the color should be set, but there are no scores <br/>
    /// 5. If the width should be set, interpolate the width of each edge between <see cref="EdgeMinWidth"/> and <see cref="EdgeMaxWidth"/> based on the score <br/>
    /// 6. If the color should not be set, skip the edge <br/>
    /// 7. Calculate the color ratio based on the score, minimum and maximum score<br/>
    /// 8. If <see cref="UseProtocolColors"/> is true and <see cref="TraceEdge.DataProtocol"/> is set in <see cref="ProtocolColors"/>, set the hex colors from it<br/>
    /// 9. Else if <see cref="DataProtocol.Unknown"/> is defined inside <see cref="ProtocolColors"/>, set the hex colors from it<br/>
    /// 10. Else, set the hex colors to use white and black as the start and end colors <br/>
    /// 11. Interpolate the hex colors based on the color ratio <br/>
    /// /// </summary>
    public void StyleEdges(ITraceGraph traceGraph)
    {
        var edges = traceGraph.Edges.Values;
        if (edges.IsNullOrEmpty())
            return;

        var widthScores = edges.Select(edge => ScoreEdge(edge, WidthScoringMode)).ToList();
        var colorScores = edges.Select(edge => ScoreEdge(edge, ColorScoringMode)).ToList();

        ulong? maxWidthScore = widthScores.Any() ? widthScores.Max() : null;
        ulong? minWidthScore = widthScores.Any() ? widthScores.Min() : null;
        ulong? maxColorScore = colorScores.Any() ? colorScores.Max() : null;
        ulong? minColorScore = colorScores.Any() ? colorScores.Min() : null;

        foreach (var edge in edges)
        {
            if (SetWidth)
                edge.Width = CalculateWidth(ScoreEdge(edge, WidthScoringMode), minWidthScore!.Value,
                    maxWidthScore!.Value);

            if (!SetColor)
                continue;

            var colorRatio = (float)(ScoreEdge(edge, ColorScoringMode) - minColorScore!.Value) /
                             (maxColorScore!.Value - minColorScore.Value);
            string startColor = "#FFFFFF", endColor = "#000000";

            if ((UseProtocolColors && 
                 ProtocolColors.TryGetValue(edge.DataProtocol, out var protocolColors)) ||
                ProtocolColors.TryGetValue(DataProtocol.Unknown, out protocolColors))
            {
                startColor = protocolColors.StartHex;
                endColor = protocolColors.EndHex;
            }

            edge.HexColor = InterpolateColors ? InterpolateHexColors(startColor, endColor, colorRatio) : endColor;
        }
    }

    private float CalculateWidth(ulong score, ulong minScore, ulong maxScore)
    {
        if (minScore == maxScore)
            return EdgeMaxWidth;
        
        var width = (float)(score - minScore) / (maxScore - minScore);
        return width * (EdgeMaxWidth - EdgeMinWidth) + EdgeMinWidth;
    }

    private static ulong ScoreEdge(TraceEdge edge, ScoringMode mode)
    {
        return mode switch
        {
            ScoringMode.PacketCount => edge.PacketCount,
            ScoringMode.ByteCount => edge.ByteCount,
            ScoringMode.Calculated => edge.PacketCount * edge.ByteCount,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string InterpolateHexColors(string hexColor1, string hexColor2, float ratio)
    {
        var (r1, g1, b1) = HexToRgb(hexColor1);
        var (r2, g2, b2) = HexToRgb(hexColor2);

        ratio = float.IsNaN(ratio) ? 1f : Math.Clamp(ratio, 0f, 1f);
        var rInterpolated = (int)(r1 + (r2 - r1) * ratio);
        var gInterpolated = (int)(g1 + (g2 - g1) * ratio);
        var bInterpolated = (int)(b1 + (b2 - b1) * ratio);

        return RgbToHex(rInterpolated, gInterpolated, bInterpolated);
    }

    private static (int, int, int) HexToRgb(string hex)
    {
        var r = Convert.ToInt32(hex.Substring(1, 2), 16);
        var g = Convert.ToInt32(hex.Substring(3, 2), 16);
        var b = Convert.ToInt32(hex.Substring(5, 2), 16);
        return (r, g, b);
    }

    private static string RgbToHex(int r, int g, int b) => $"#{r:X2}{g:X2}{b:X2}";
}

public record EdgeStylerDto(
    bool SetWidth,
    ScoringMode WidthScoringMode,
    float EdgeMinWidth,
    float EdgeMaxWidth,
    bool SetColor,
    ScoringMode ColorScoringMode,
    bool InterpolateColors,
    bool UseProtocolColors,
    Dictionary<DataProtocol, ColorRange> ProtocolColors);

public record NodeColorAssignment(IpAddressMatcher Matcher, string? HexColor);
public record NodeColorAssignmentDto(IpAddressMatcherDto Matcher, string? HexColor);

public class NodeStyler
{
    [BsonElement("setColor")]
    public bool SetColor { get; set; }
    
    [BsonElement("hexColor")]
    public List<NodeColorAssignment> Assignments { get; set; } = new();
    
    /// <summary>
    /// Set the <see cref="TraceNode.HexColor"/> attribute based on the following rules: <br/>
    /// 1. If <see cref="SetColor"/> is false, do nothing <br/>
    /// 2. Get the first match for the node's address in <see cref="Assignments"/> <br/>
    /// 3. If there is no match, skip the node <br/>
    /// 4. If the color is null, skip the node <br/>
    /// 5. Set the node's <see cref="TraceNode.HexColor"/> to the color
    /// </summary>
    /// <param name="traceGraph"></param>
    public void StyleNodes(ITraceGraph traceGraph)
    {
        if (!SetColor)
            return;
        
        var nodes = traceGraph.Nodes.Values;

        foreach (var node in nodes)
        {
            var assignment = Assignments.FirstOrDefault(
                a => a.Matcher.Match(node.Address.GetAddressBytes()));

            if (assignment?.HexColor is null)
                continue;
            
            node.HexColor = assignment.HexColor;
        }
    }
}

public record NodeStylerDto(bool SetColor, List<NodeColorAssignmentDto> Assignments);

public class StylingLayer : ILayer
{
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("type")]
    public string Type { get; set; } = LayerType.Styling;
    
    [BsonElement("enabled")]
    public bool Enabled { get; set; }
    
    [BsonIgnore]
    public string Description => "";
    
    [BsonElement("edgeStyler")]
    public EdgeStyler EdgeStyler { get; set; } = new();
    
    [BsonElement("nodeStyler")]
    public NodeStyler NodeStyler { get; set; } = new();

    public void Execute(ITraceGraph graph, IServiceProvider _)
    {
        EdgeStyler.StyleEdges(graph);
        NodeStyler.StyleNodes(graph);
    }
}

public record StylingLayerDto(string Type, bool Enabled, string? Name, EdgeStylerDto EdgeStyler, NodeStylerDto NodeStyler) : ILayerDto;

public class ProtocolColorsDictionarySerializer : IBsonSerializer
{
    public Type ValueType => typeof(Dictionary<DataProtocol, ColorRange>);

    public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var document = BsonDocumentSerializer.Instance.Deserialize(context, args);
        var dictionary = new Dictionary<DataProtocol, ColorRange>();

        foreach (var element in document)
        {
            if (Enum.TryParse<DataProtocol>(element.Name, out var key))
            {
                var valueTuple = element.Value.AsBsonDocument;
                var startHex = valueTuple["StartHex"].AsString;
                var endHex = valueTuple["EndHex"].AsString;
                dictionary[key] = new ColorRange(startHex, endHex);
            }
        }

        return dictionary;
    }

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        var dictionary = (Dictionary<DataProtocol, ColorRange>)value;
        var document = new BsonDocument();

        foreach (var kvp in dictionary)
        {
            var keyString = kvp.Key.ToString();
            var valueDocument = new BsonDocument
            {
                { "StartHex", kvp.Value.StartHex },
                { "EndHex", kvp.Value.EndHex }
            };

            document.Add(keyString, valueDocument);
        }

        BsonDocumentSerializer.Instance.Serialize(context, document);
    }
}