namespace ConferenceRooms.DTOs.Booking
{
    public class BookingResponse
    {
        public long BookingId { get; set; }

        public long RoomId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal BasePrice { get; set; }

        public decimal Discount { get; set; }

        public decimal Surcharge { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = string.Empty;

        public List<long> ServiceIds { get; set; } = new();
    }
}
