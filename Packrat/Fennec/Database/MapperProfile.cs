using System.Diagnostics;
using System.Net;
using AutoMapper;
using Fennec.Database.Domain;
using Fennec.Database.Domain.Layers;

namespace Fennec.Database;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // TODO: rethink all this dto madness
        CreateMap<FilterConditionDto, FilterCondition>()
            .ConstructUsing((dto, _) =>
            {
                var srcAdd = IPAddress.Parse(dto.SourceAddress).GetAddressBytes();
                var srcMask = IPAddress.Parse(dto.SourceAddressMask).GetAddressBytes();
                ushort? srcPort  = dto.SourcePort == null ? null : ushort.Parse(dto.SourcePort);
                var dstAdd = IPAddress.Parse(dto.DestinationAddress).GetAddressBytes();
                var dstMask = IPAddress.Parse(dto.DestinationAddressMask).GetAddressBytes();
                ushort? dstPort = dto.DestinationPort == null ? null : ushort.Parse(dto.DestinationPort);
                TraceProtocol? protocol = dto.Protocol == null ? null : Enum.Parse<TraceProtocol>(dto.Protocol);
                return new FilterCondition(srcAdd, srcMask, srcPort, dstAdd, dstMask, dstPort, protocol, dto.Include);
            })
            .ForMember(dest => dest.SourceAddress, opt => opt.Ignore())
            .ForMember(dest => dest.SourceAddressMask, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddress, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddressMask, opt => opt.Ignore());
        ;

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
            .ForMember(dest => dest.SourceAddress, opt => opt.Ignore())
            .ForMember(dest => dest.SourceAddressMask, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddress, opt => opt.Ignore())
            .ForMember(dest => dest.DestinationAddressMask, opt => opt.Ignore());
        

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
                ctx.Mapper.Map<FilterListDto>(layer.FilterList)));
        ;

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