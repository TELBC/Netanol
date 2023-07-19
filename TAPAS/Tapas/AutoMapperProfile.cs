using AutoMapper;
using Tapas.Models;

namespace Tapas;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<SingleTrace, SingleTraceDto>();
    }
}