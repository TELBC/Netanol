using System.Net;
using AutoMapper;
using Elasticsearch.Net.Specification.IndicesApi;
using Fennec.Controllers;
using Fennec.Database.Domain;
using Fennec.Database.Domain.Layers;

namespace Fennec.Database;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // TODO: rethink all this dto madness
        CreateMap<FilterConditionDto, FilterCondition>()
            .ConstructUsing((layer, _) =>
            {
                var srcAdd = IPAddress.Parse(layer.SourceAddress).GetAddressBytes();
                var srcMask = IPAddress.Parse(layer.SourceAddressMask).GetAddressBytes();
                var dstAdd = IPAddress.Parse(layer.DestinationAddress).GetAddressBytes();
                var dstMask = IPAddress.Parse(layer.DestinationAddressMask).GetAddressBytes();
                return new FilterCondition(srcAdd, srcMask, dstAdd, dstMask, layer.Include);
            })
            .ForMember(dest => dest.SourceAddress, opt => opt.Ignore())
            .ForMember(dest => dest.SourceAddressMask, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddress, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddressMask, opt => opt.Ignore());;
        
        CreateMap<FilterCondition, FilterConditionDto>()
            .ConstructUsing((layer, _) =>
            {
                var srcAdd = new IPAddress(layer.SourceAddress);
                var srcMask = new IPAddress(layer.SourceAddressMask);
                var dstAdd = new IPAddress(layer.DestinationAddress);
                var dstMask = new IPAddress(layer.DestinationAddressMask);
                return new FilterConditionDto(srcAdd.ToString(), srcMask.ToString(), dstAdd.ToString(), dstMask.ToString(), layer.Include);
            })
            .ForMember(dest => dest.SourceAddress, opt => opt.Ignore())
            .ForMember(dest => dest.SourceAddressMask, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddress, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddressMask, opt => opt.Ignore());;
        
        CreateMap<FilterList, FilterListDto>()
            .ConstructUsing((layer, ctx) => new FilterListDto(
                layer.Conditions.Select(condition => ctx.Mapper.Map<FilterConditionDto>(condition)).ToList(),
                layer.ImplicitInclude));
        
        CreateMap<FilterListDto, FilterList>()
            .ConstructUsing((layer, ctx) => new FilterList(
                layer.ImplicitInclude,
                layer.Conditions.Select(condition => ctx.Mapper.Map<FilterCondition>(condition)).ToList()));

        CreateMap<FilterLayer, ILayerDto>()
            .ConstructUsing((layer, ctx) => new FilterLayerDto(
                layer.Type,
                layer.Name,
                layer.Enabled,
                ctx.Mapper.Map<FilterListDto>(layer.FilterList)));;
        
        CreateMap<FilterLayer, FilterLayerDto>()
            .ConstructUsing((layer, ctx) => new FilterLayerDto(
                layer.Type,
                layer.Name,
                layer.Enabled,
                ctx.Mapper.Map<FilterListDto>(layer.FilterList)));
        
        CreateMap<FilterLayerDto, FilterLayer>()
            .ConstructUsing((layer, ctx) => new FilterLayer(
                layer.Name,
                layer.Enabled,
                ctx.Mapper.Map<FilterList>(layer.FilterList)));

        CreateMap<ILayer, ShortLayerDto>();

        CreateMap<ILayerDto, ILayer>()
            .ConstructUsing((layer, ctx) =>
            {
                return layer switch
                {
                    FilterLayerDto filterLayer => ctx.Mapper.Map<FilterLayer>(filterLayer),
                    _ => throw new ArgumentException("Unknown layer type")
                };
            });
        
        CreateMap<Layout, ShortLayoutDto>()
            .ConstructUsing(src => new ShortLayoutDto(src.Name, src.Layers.Count));

        CreateMap<Layout, FullLayoutDto>()
            .ConstructUsing((src, ctx) => new FullLayoutDto(src.Name, 
                src.Layers.Select(layer => ctx.Mapper.Map<ShortLayerDto>(layer)).ToList()));
    }
}