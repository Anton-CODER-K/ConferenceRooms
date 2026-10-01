namespace ConferenceRooms.DTOs.Booking
{
    public class CreateBookingRequest
    {
        public long RoomId { get; set; }

        public DateTime StartTime { get; set; }

        public int DurationMinutes { get; set; }

        public List<long> ServiceIds { get; set; } = new();
    }
}
