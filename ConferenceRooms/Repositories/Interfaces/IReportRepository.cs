using ConferenceRooms.Entities;
using Npgsql;

namespace ConferenceRooms.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<List<RoomReport>> GetRoomReport(DateTime from, DateTime to, NpgsqlConnection connection, NpgsqlTransaction? transaction = null);
    }
}
