namespace ConferenceRooms.Entities
{
    public class Booking
    {
        public long BookingId { get; set; }

        public long RoomId { get; set; }

        public long UserId { get; set; }

        public short StatusId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal BasePrice { get; set; }

        public decimal Discount { get; set; }

        public decimal Surcharge { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
