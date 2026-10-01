namespace ConferenceRooms.Entities
{
    public class RoomReport
    {
        public long RoomId { get; set; }

        public string RoomName { get; set; } = string.Empty;

        public int BookingCount { get; set; }

        public decimal BookedHours { get; set; }

        public decimal Revenue { get; set; }
    }
}
