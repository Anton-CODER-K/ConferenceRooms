using System.ComponentModel.DataAnnotations;

namespace ConferenceRooms.DTOs.Auth
{
    public class RegisterRequest
    {
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
