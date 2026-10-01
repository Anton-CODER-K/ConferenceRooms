using ConferenceRooms.DTOs.Reports;
using ConferenceRooms.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Controllers
{
    [ApiController]
    [Route("reports")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("rooms")]
        public async Task<ActionResult<RoomReportResponse>> GetRoomReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _reportService.GetRoomReportAsync(from, to);

            return Ok(result);
        }
    }
}
