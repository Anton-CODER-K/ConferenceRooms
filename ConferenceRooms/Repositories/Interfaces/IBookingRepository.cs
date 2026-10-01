using ConferenceRooms.Entities;
using Npgsql;

namespace ConferenceRooms.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<bool> HasOverlappingBooking(long roomId, DateTime startTime, DateTime endTime, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task<long> CreateBooking(Booking booking, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task InsertBookingService(long bookingId, long serviceId, decimal price, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task<List<long>> GetRoomServiceIds(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
    }
}
