using System.Net;
using AutoMapper;
using Fennec.Database.Domain;
using Fennec.Processing;

namespace Fennec.Database;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // TODO: rethink all this dto madness
        CreateMap<IPAddress, string>()
            .ConvertUsing(ip => ip.ToString());
        
        CreateMap<string, IPAddress>()
            .ConvertUsing(str => IPAddress.Parse(str));
        
        CreateMap<string, byte[]>()
            .ConvertUsing(str => IPAddress.Parse(str).GetAddressBytes());
        
        CreateMap<byte[], string>()
            .ConvertUsing(bytes => new IPAddress(bytes).ToString());
        
        CreateMap<FilterConditionDto, FilterCondition>()
            .ConstructUsing((dto, _) =>
            {
                var srcAdd = IPAddress.Parse(dto.SourceAddress).GetAddressBytes();
                var srcMask = IPAddress.Parse(dto.SourceAddressMask).GetAddressBytes();
                ushort? srcPort = dto.SourcePort == null ? null : ushort.Parse(dto.SourcePort);
                var dstAdd = IPAddress.Parse(dto.DestinationAddress).GetAddressBytes();
                var dstMask = IPAddress.Parse(dto.DestinationAddressMask).GetAddressBytes();
                ushort? dstPort = dto.DestinationPort == null ? null : ushort.Parse(dto.DestinationPort);
                DataProtocol? protocol = dto.Protocol == null ? null : Enum.Parse<DataProtocol>(dto.Protocol);
                return new FilterCondition(srcAdd, srcMask, srcPort, dstAdd, dstMask, dstPort, protocol, dto.Include);
            })
            .ForAllMembers(m => m.AllowNull());

        CreateMap<FilterCondition, FilterConditionDto>()
            .ConstructUsing((filterCondition, _) =>
            {
                var srcAdd = new IPAddress(filterCondition.SourceAddress);
                var srcMask = new IPAddress(filterCondition.SourceAddressMask);
                var dstAdd = new IPAddress(filterCondition.DestinationAddress);
                var dstMask = new IPAddress(filterCondition.DestinationAddressMask);
                return new FilterConditionDto(
                    srcAdd.ToString(),
                    srcMask.ToString(),
                    filterCondition.SourcePort?.ToString(),
                    dstAdd.ToString(),
                    dstMask.ToString(),
                    filterCondition.DestinationPort?.ToString(),
                    filterCondition.Protocol?.ToString(),
                    filterCondition.Include);
            })
            .ForAllMembers(m => m.AllowNull());


        CreateMap<FilterList, FilterListDto>();
        CreateMap<FilterListDto, FilterList>();

        CreateMap<FilterLayer, ILayerDto>();
        CreateMap<FilterLayer, FilterLayerDto>();
        CreateMap<FilterLayerDto, FilterLayer>();

        CreateMap<ILayer, ShortLayerDto>();
        
        CreateMap<ILayerDto, ILayer>()
            .ConstructUsing((dto, ctx) =>
            {
                if (!LayerType.LookupTable.TryGetValue(dto.Type, out var layerType))
                    throw new ArgumentException("No layer type found in the lookup table.");
                
                return (ILayer) ctx.Mapper.Map(dto, dto.GetType(), layerType.LayerType);
            });
        
        CreateMap<ILayer, ILayerDto>()
            .ConstructUsing((layer, ctx) =>
            {
                if (!LayerType.LookupTable.TryGetValue(layer.Type, out var layerType))
                    throw new ArgumentException("No layer type found in the lookup table.");
                
                return (ILayerDto) ctx.Mapper.Map(layer, layer.GetType(), layerType.DtoType);
            });

        CreateMap<Layout, ShortLayoutDto>()
            .ForCtorParam("LayerCount", opt => 
                opt.MapFrom(src => src.Layers.Count));

        CreateMap<Layout, FullLayoutDto>();

        CreateMap<QueryConditions, QueryConditionsDto>()
            .ForAllMembers(opts => opts.AllowNull());

        CreateMap<QueryConditionsDto, QueryConditions>()
            .ForAllMembers(opts => opts.AllowNull());

        CreateMap<AggregationLayer, AggregationLayerDto>();
        CreateMap<AggregationLayerDto, AggregationLayer>();
        CreateMap<IpAddressMatcher, IpAddressMatcherDto>();
        CreateMap<IpAddressMatcherDto, IpAddressMatcher>();

        CreateMap<VmwareTaggingLayer, VmwareTaggingLayerDto>();
        CreateMap<VmwareTaggingLayerDto, VmwareTaggingLayer>();
        
        CreateMap<TagFilterLayer, TagFilterLayerDto>();
        CreateMap<TagFilterLayerDto, TagFilterLayer>();
        CreateMap<TagFilterCondition, TagFilterConditionDto>();
        CreateMap<TagFilterConditionDto, TagFilterCondition>();

        CreateMap<NamingLayer, NamingLayerDto>();
        CreateMap<NamingLayerDto, NamingLayer>();
        CreateMap<NamingAssigner, NamingLayerDto>();
        CreateMap<NamingLayerDto, NamingAssigner>();

        CreateMap<EdgeStyler, EdgeStylerDto>();
        CreateMap<EdgeStylerDto, EdgeStyler>();
        CreateMap<NodeColorAssignment, NodeColorAssignmentDto>();
        CreateMap<NodeColorAssignmentDto, NodeColorAssignment>();
        CreateMap<NodeStyler, NodeStylerDto>();
        CreateMap<NodeStylerDto, NodeStyler>();
        CreateMap<StylingLayer, StylingLayerDto>();
        CreateMap<StylingLayerDto, StylingLayer>();
    }
}