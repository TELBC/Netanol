using System.Net;
using AutoMapper;
using Elasticsearch.Net.Specification.RollupApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        
        [HttpGet("get_by_window")]
        public async Task<IActionResult> GetTracesByWindow([FromQuery(Name = "from")] DateTimeOffset from,
            [FromQuery(Name = "until")] DateTimeOffset until)
        {
            return Ok(await _traceRepository.GroupTracesByTimeSpanAndReturnAsDto(from, until));
        }
    }
}