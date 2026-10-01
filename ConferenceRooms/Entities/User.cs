using ConferenceRooms.Enums;

namespace ConferenceRooms.Entities
{
    public class User
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public Role RoleId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
