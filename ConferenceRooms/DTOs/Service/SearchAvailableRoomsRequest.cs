namespace ConferenceRooms.DTOs.Service
{
    public class SearchAvailableRoomsRequest
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
    }
}
