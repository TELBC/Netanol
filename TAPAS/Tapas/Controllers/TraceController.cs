using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Tapas.Database;
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
            var traceDtos = _mapper.Map<IEnumerable<SingleTraceDto>>(traces); // this uses AutoMapper to map the traces to SingleTraceDto

            var groupedTraces = traceDtos.GroupBy(dto => dto, new SingleTraceDtoEqualityComparer())
                .Select(group => new
                {
                    Trace = group.Key,
                    Count = group.Count()
                });

            return Ok(groupedTraces);
        }
    }
}