namespace ConferenceRooms.DTOs.Room
{
    public class CreateRoomRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal HourlyRate { get; set; }
        public List<long> ServiceIds { get; set; } = new();

    }
}
