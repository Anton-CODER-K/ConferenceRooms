using ConferenceRooms.Entities;
using Npgsql;

namespace ConferenceRooms.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        public Task<long> CreateRoom(Room room, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task InsertServicesToRoom(List<long> serviceIds, long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task<Room?> GetRoomById(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task UpdateRoom(Room room, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task DeleteServicesToRoom(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task DeleteRoom(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task<List<Room>> SearchAvailableRooms(DateTime startTime, DateTime endTime, int capacity, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
    }
}
