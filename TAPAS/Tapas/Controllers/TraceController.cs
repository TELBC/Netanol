using System.Globalization;
using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Tapas.Database;
using Tapas.Database.Dto;
using Tapas.Models;

namespace Tapas.Controllers
{
    [Route("trace")]
    public class TraceController : ControllerBase
    {
        private readonly TraceRepository _traceRepository;
        private readonly IMapper _mapper;

        public TraceController(TraceRepository traceRepository, IMapper mapper)
        {
            _traceRepository = traceRepository;
            _mapper = mapper;
        }

        [HttpGet("/get_all")]
        public async Task<IActionResult> GetAllTraces()
        {
            var traces = await _traceRepository.GetAllSingleTraces();
            var traceDtos = _mapper.Map<IEnumerable<SingleTraceDto>>(traces);

            var groupedTraces = traceDtos.GroupBy(dto => dto, new SingleTraceDtoEqualityComparer())
                .Select(group => new
                {
                    Trace = new SingleTraceDto(
                        group.Key.Protocol,
                        IPAddress.Parse(group.Key.SourceIpAddress).ToString(), // Convert back to IPAddress
                        group.Key.SourcePort,
                        IPAddress.Parse(group.Key.DestinationIpAddress).ToString(), // Convert back to IPAddress
                        group.Key.DestinationPort
                    ),
                    Count = group.Count()
                });

            return Ok(groupedTraces);
        }
        
        [HttpGet("/get_by_window")]
        public async Task<IActionResult> GetTracesByWindow([FromQuery(Name = "from")] DateTimeOffset from, [FromQuery(Name = "until")] DateTimeOffset until)
        {
            var traces = await _traceRepository.GetTracesByTimestamp(from, until);
            var traceDtos = _mapper.Map<IEnumerable<SingleTraceDto>>(traces);
            
            var groupedTraces = traceDtos.GroupBy(dto => dto, new SingleTraceDtoEqualityComparer())
                .Select(group => new
                {
                    Trace = new SingleTraceDto(
                        group.Key.Protocol,
                        IPAddress.Parse(group.Key.SourceIpAddress).ToString(), // Convert back to IPAddress
                        group.Key.SourcePort,
                        IPAddress.Parse(group.Key.DestinationIpAddress).ToString(), // Convert back to IPAddress
                        group.Key.DestinationPort
                    ),
                    Count = group.Count()
                });
            
            return Ok(groupedTraces);
        }
    }
}