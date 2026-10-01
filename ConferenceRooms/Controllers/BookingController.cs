using ConferenceRooms.DTOs.Booking;
using ConferenceRooms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConferenceRooms.Controllers
{
    [ApiController]
    [Route("/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly BookingService _bookingService;

        public BookingController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!long.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var result = await _bookingService.CreateBookingAsync(userId, request);

            return StatusCode(StatusCodes.Status201Created, result);

        }
    }
}
