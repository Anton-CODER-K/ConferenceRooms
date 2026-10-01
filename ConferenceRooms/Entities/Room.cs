namespace ConferenceRooms.Entities
{
    public class Room
    {
        public long RoomId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<long> ServiceIds { get; set; } = new();
    }
}
