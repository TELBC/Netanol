using Fennec.Processing.Graph;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fennec.Processing;

public static class LayerType
{
    public const string Filter = "filter";
    public const string Aggregation = "aggregation";
    public const string Naming = "naming";
    public const string Styling = "styling";
    public const string VmwareTagging = "vmware-tagging";
    public const string TagFilter = "tag-filter";

    public static readonly Dictionary<string, (Type LayerType, Type DtoType)> LookupTable = new()
    {
        { Filter, (typeof(FilterLayer), typeof(FilterLayerDto)) },
        { Aggregation, (typeof(AggregationLayer), typeof(AggregationLayerDto)) },
        { Naming, (typeof(NamingLayer), typeof(string)) },
        { VmwareTagging, (typeof(VmwareTaggingLayer), typeof(VmwareTaggingLayerDto)) },
        { TagFilter, (typeof(TagFilterLayer), typeof(TagFilterLayerDto)) }
    };
}

/// <summary>
///     A single processing step performed on the graph before sending it to the frontend.
/// </summary>
[JsonConverter(typeof(JsonLayerSerializer))]
public interface ILayer
{
    /// <summary>
    ///     An optional name that can be set by the user for easier identification.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    ///     The type of the layer.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    ///     Sets whether this layer should be executed or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     A short description of the layer that is displayed in the frontend.
    /// </summary>
    [BsonIgnore]
    public string Description { get; }

    public void Execute(ITraceGraph graph, IServiceProvider serviceProvider);
}

/// <summary>
///     Represents a simplified layout with its data replaced by a short description.
/// </summary>
/// <param name="Type"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record ShortLayerDto(string Type, string Name, bool Enabled, string Description);

/// <summary>
///     Fully fledged layer with an arbitrary data object.
/// </summary>
public interface ILayerDto
{
    public string Type { get; }
    public string? Name { get; }
    public bool Enabled { get; }
}

public class LayerModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        return context.Metadata.ModelType == typeof(ILayerDto) ? new LayerModelBinder() : null;
    }
}

public class LayerModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var jsonString = bindingContext.ActionContext.HttpContext.Request.Body;
        var jsonObject = await JToken.ReadFromAsync(new JsonTextReader(new StreamReader(jsonString)));

        var type = jsonObject["type"]?.ToString();
        if (type == null)
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                "Can not parse JSON to ILayer without `type` attribute to distinguish.");
            return;
        }

        if (!LayerType.LookupTable.TryGetValue(type, out var typeType))
        {
            bindingContext.ModelState.AddModelError(bindingContext.ModelName,
                $"The layer type {type} is not supported and must be one of {LayerType.LookupTable.Keys}.");
            return;
        }

        var layer = jsonObject.ToObject(typeType.DtoType);
        bindingContext.Result = ModelBindingResult.Success(layer);
    }
}

public class MongoLayerSerializer : SerializerBase<ILayer>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ILayer value)
    {
        if (!LayerType.LookupTable.TryGetValue(value.Type, out var typeType))
            throw new NotSupportedException($"Unknown layer type: {value.Type}");

        BsonSerializer.Serialize(context.Writer, typeType.LayerType, value);
    }

    public override ILayer Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var document = BsonDocumentSerializer.Instance.Deserialize(context);
        var type = document["type"].AsString;

        if (!LayerType.LookupTable.TryGetValue(type, out var typeType))
            throw new NotSupportedException($"Unknown layer type: {type}");

        return (ILayer)BsonSerializer.Deserialize(document, typeType.LayerType);
    }
}

public class JsonLayerSerializer : JsonConverter<ILayerDto>
{
    public override bool CanWrite => false; // Set to true if you implement the WriteJson method

    public override void WriteJson(JsonWriter writer, ILayerDto? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override ILayerDto ReadJson(JsonReader reader, Type objectType, ILayerDto? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["type"]?.Value<string>();
        if (type == null)
            throw new FormatException("Can not parse JSON to ILayer without `type` attribute to distinguish.");

        if (!LayerType.LookupTable.TryGetValue(type, out var typeType))
            throw new NotSupportedException($"Unknown layer type: {type}");

        return (ILayerDto)jsonObject.ToObject(typeType.DtoType)!;
    }
}