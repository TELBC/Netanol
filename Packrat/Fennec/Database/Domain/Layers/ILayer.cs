using Fennec.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Fennec.Database.Domain.Layers;

public static class LayerType
{
    public const string Filter = "filter";
    public const string Aggregation = "aggregation";
    public const string Name = "name";
    public const string Position = "position";
}

/// <summary>
/// A single processing step performed on the graph before sending it to the frontend.
/// </summary>
[JsonConverter(typeof(JsonLayerSerializer))]
public interface ILayer
{
    /// <summary>
    /// An optional name that can be set by the user for easier identification.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The type of the layer.
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Sets whether this layer should be executed or not.
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// A short description of the layer that is displayed in the frontend.
    /// </summary>
    [BsonIgnore]
    public string Description { get;  }
    
    public void Execute(ref List<AggregateTrace> aggregateTraces);
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
    public string Name { get; }
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

        ILayerDto layer;

        // Here you should implement logic to determine the concrete type
        // For example, based on the "Type" property in JSON
        var type = jsonObject["Type"]?.ToString();
        switch (type)
        {
            case LayerType.Filter:
                var optLayer = jsonObject.ToObject<FilterLayerDto>();
                if (optLayer == null)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Failed to parse layer");
                    return;
                }
                layer = optLayer;
                break;
            // Add cases for other types
            default:
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Unknown layer type");
                return;
        }

        bindingContext.Result = ModelBindingResult.Success(layer);
    }
}

public class MongoLayerSerializer : SerializerBase<ILayer>
{
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ILayer value)
    {
        if (value is FilterLayer filterLayer)
            BsonSerializer.Serialize(context.Writer, filterLayer);
    }

    public override ILayer Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var document = BsonDocumentSerializer.Instance.Deserialize(context);
        var type = document["type"].AsString;

        return type switch
        {
            LayerType.Filter => BsonSerializer.Deserialize<FilterLayer>(document),
            LayerType.Aggregation => BsonSerializer.Deserialize<AggregationLayer>(document),
            LayerType.Name => BsonSerializer.Deserialize<NameLayer>(document),
            LayerType.Position => BsonSerializer.Deserialize<PositionLayer>(document),
            _ => throw new NotSupportedException($"Unknown layer type: {type}")
        };
    }
}

public class JsonLayerSerializer : JsonConverter<ILayerDto>
{
    public override void WriteJson(JsonWriter writer, ILayerDto? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }

    public override ILayerDto? ReadJson(JsonReader reader, Type objectType, ILayerDto? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var jsonObject = JObject.Load(reader);
        var type = jsonObject["Type"]?.Value<string>();

        if (type == null)
            throw new FormatException("Can not parse JSON to ILayer without Type attribute to distinguish.");

        return type switch
        {
            LayerType.Filter => jsonObject.ToObject<FilterLayerDto>(),
            // LayerType.Aggregation => jsonObject.ToObject<AggregationLayer>(),
            // LayerType.Name => jsonObject.ToObject<NameLayer>(),
            // LayerType.Position => jsonObject.ToObject<PositionLayer>(),
            _ => throw new NotSupportedException($"Unknown layer type: {type}")
        };
    }

    public override bool CanWrite => false; // Set to true if you implement the WriteJson method
}