using System.Text.RegularExpressions;
using Fennec.Processing.Graph;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fennec.Processing;

/// <summary>
///     Filters nodes by their tags.
/// </summary>
public class TagFilterLayer : ILayer
{
    [BsonElement("implicitInclude")]
    public bool ImplicitInclude { get; init; }

    [BsonElement("conditions")]
    public List<TagFilterCondition> Conditions { get; init; } = default!;

    [BsonElement("type")]
    public string Type { get; set; } = LayerType.TagFilter;

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("enabled")]
    public bool Enabled { get; set; }

    [BsonIgnore]
    public string Description {
        get
        {
            var impl = ImplicitInclude ? "Include" : "Exclude";
            var cond = Conditions.Count == 1 ? "Condition" : "Conditions";
            return $"{Conditions.Count} <b>{cond}</b>, Implicit <b>{impl}</b>";
        }
    }

    public void Execute(ITraceGraph graph, IServiceProvider _)
    {
        graph.FilterNodes(n =>
        {
            var condition = Conditions.FirstOrDefault(c => c.Match(n));
            return condition?.Include ?? ImplicitInclude;
        });
    }
}

public record TagFilterLayerDto(string Type, string? Name, bool Enabled, bool ImplicitInclude, List<TagFilterConditionDto> Conditions) : ILayerDto;

public enum TagFilterConditionType
{
    MatchesNone,
    MatchesAny,
    MatchesAll,
    MatchesExactly
}

/// <summary>
///     Matches nodes based on their tags.
/// </summary>
public class TagFilterCondition
{
    [BsonIgnore]
    private List<Regex>? _compiledRegexes;

    [BsonElement("type")]
    [BsonRepresentation(BsonType.String)]
    public TagFilterConditionType Type { get; init; }

    [BsonElement("regexes")]
    public List<string> Regexes { get; init; } = default!;

    [BsonIgnore]
    private List<Regex> CompiledRegexes
    {
        get
        {
            if (_compiledRegexes != null)
                return _compiledRegexes;

            _compiledRegexes = Regexes.Select(r => new Regex(r, RegexOptions.Compiled)).ToList();
            return _compiledRegexes;
        }
    }

    [BsonElement("include")]
    public bool Include { get; init; }

    public bool Match(TraceNode node)
    {
        return Type switch
        {
            TagFilterConditionType.MatchesNone when node.Tags.IsNullOrEmpty() => true,
            TagFilterConditionType.MatchesNone => !CompiledRegexes.Any(regex => node.Tags?.Any(regex.IsMatch) ?? false),

            TagFilterConditionType.MatchesAny when node.Tags.IsNullOrEmpty() => false,
            TagFilterConditionType.MatchesAny => CompiledRegexes.Any(regex => node.Tags?.Any(regex.IsMatch) ?? false),

            TagFilterConditionType.MatchesAll when node.Tags.IsNullOrEmpty() => false,
            TagFilterConditionType.MatchesAll => CompiledRegexes.All(regex => node.Tags?.Any(regex.IsMatch) ?? false),

            TagFilterConditionType.MatchesExactly when node.Tags.IsNullOrEmpty() => CompiledRegexes.Count == 0,
            TagFilterConditionType.MatchesExactly => CompiledRegexes.Count == node.Tags?.Count &&
                                                 CompiledRegexes.All(regex => node.Tags?.Any(regex.IsMatch) ?? false),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public record TagFilterConditionDto(TagFilterConditionType Type, List<string> Regexes, bool Include);