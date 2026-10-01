namespace ConferenceRooms.DTOs.Room
{
    public class UpdateRoomRequest
    {
        public string? Name { get; set; }
        public int? Capacity { get; set; }
        public decimal? HourlyRate { get; set; }
        public List<long>? ServiceIds { get; set; }
    }
}
