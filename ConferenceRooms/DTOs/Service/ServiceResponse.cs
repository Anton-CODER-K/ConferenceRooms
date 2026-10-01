namespace ConferenceRooms.DTOs.Service
{
    public class ServiceResponse
    {
        public long ServiceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
