using ConferenceRooms.DTOs.Auth;
using ConferenceRooms.DTOs.Room;
using ConferenceRooms.DTOs.Service;
using ConferenceRooms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Controllers
{
    [ApiController]
    [Route("/rooms")]
    public class RoomController : ControllerBase
    {
        private readonly RoomService _roomService;

        public RoomController(RoomService roomService)
        {
            _roomService = roomService;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<long>> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var roomId = await _roomService.CreateRoomAsync(request);

            return StatusCode(StatusCodes.Status201Created, roomId);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{roomId}")]
        public async Task<IActionResult> UpdateRoom(long roomId, [FromBody] UpdateRoomRequest request)
        {
            await _roomService.UpdateRoomAsync(roomId, request);

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{roomId}")]
        public async Task<IActionResult> DeleteRoom(long roomId)
        {
            await _roomService.DeleteRoomAsync(roomId);

            return NoContent();
        }

        [HttpGet("available")]
        public async Task<ActionResult<List<RoomResponse>>> SearchAvailableRooms([FromQuery] SearchAvailableRoomsRequest request)
        {
            var rooms = await _roomService.SearchAvailableRoomsAsync(request);

            return Ok(rooms);
        }
    }
}
