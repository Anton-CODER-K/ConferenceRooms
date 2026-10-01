using ConferenceRooms.DTOs.Booking;
using ConferenceRooms.Entities;
using Npgsql;

namespace ConferenceRooms.Services.Interfaces
{
    public interface IPricingService
    {
        Task<PricingResult> Calculate(decimal hourlyRate, DateTime startTime, DateTime endTime, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
    }
}
