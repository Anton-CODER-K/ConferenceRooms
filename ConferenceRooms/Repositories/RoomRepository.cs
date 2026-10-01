using ConferenceRooms.Entities;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace ConferenceRooms.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        public async Task<long> CreateRoom(Room room, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                INSERT INTO rooms
                (
                    name,
                    capacity,
                    hourly_rate,
                    is_active
                )
                VALUES
                (
                    @name,
                    @capacity,
                    @hourly_rate,
                    @is_active
                )
                RETURNING room_id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@name", NpgsqlDbType.Text).Value = room.Name;
            cmd.Parameters.Add("@capacity", NpgsqlDbType.Integer).Value = room.Capacity;
            cmd.Parameters.Add("@hourly_rate", NpgsqlDbType.Numeric).Value = room.HourlyRate;
            cmd.Parameters.Add("@is_active", NpgsqlDbType.Boolean).Value = true;

            var result = await cmd.ExecuteScalarAsync();

            if (result is null || result == DBNull.Value)
                throw new Exception("Failed to create room.");

            var roomId = Convert.ToInt64(result);

            return roomId;
        }


        public async Task InsertServicesToRoom(List<long> serviceIds, long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                INSERT INTO room_services
                (
                    room_id,
                    service_id
                )
                SELECT
                    @room_id,
                    service_id
                FROM unnest(@service_ids) AS service_id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@room_id", NpgsqlDbType.Bigint).Value = roomId;
            cmd.Parameters.Add("@service_ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint).Value = serviceIds.ToArray();

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Room?> GetRoomById(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT
                    room_id,
                    name,
                    capacity,
                    hourly_rate,
                    is_active,
                    created_at,
                    updated_at
                FROM rooms
                WHERE room_id = @room_id
                Limit 1;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@room_id", NpgsqlDbType.Bigint).Value = roomId;

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Room
                {
                    RoomId = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Capacity = reader.GetInt32(2),
                    HourlyRate = reader.GetDecimal(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5),
                    UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                };
            }

            return null;
        }

        public async Task UpdateRoom(Room room, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                UPDATE rooms
                SET
                    name = @name,
                    capacity = @capacity,
                    hourly_rate = @hourly_rate,
                    updated_at = CURRENT_TIMESTAMP
                WHERE room_id = @room_id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@room_id", NpgsqlDbType.Bigint).Value = room.RoomId;
            cmd.Parameters.Add("@name", NpgsqlDbType.Text).Value = room.Name;
            cmd.Parameters.Add("@capacity", NpgsqlDbType.Integer).Value = room.Capacity;
            cmd.Parameters.Add("@hourly_rate", NpgsqlDbType.Numeric).Value = room.HourlyRate;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteServicesToRoom(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string deleteSql = """
                DELETE FROM room_services
                WHERE room_id = @room_id;
                """;

            await using var cmd = new NpgsqlCommand(deleteSql, conn, tx);

            cmd.Parameters.Add("@room_id", NpgsqlDbType.Bigint).Value = roomId;

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteRoom(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                Update rooms
                Set is_active = false
                Where room_id = @room_id 
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@room_id", NpgsqlDbType.Bigint).Value = roomId;

            if (await cmd.ExecuteNonQueryAsync() == 0)
            {
                throw new BusinessException("Room not found", StatusCodes.Status404NotFound);
            }
        }

        public async Task<List<Room>> SearchAvailableRooms(DateTime startTime, DateTime endTime, int capacity, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT
                    r.room_id,
                    r.name,
                    r.capacity,
                    r.hourly_rate,
                    r.is_active,
                    r.created_at,
                    r.updated_at,
                    COALESCE(
                        ARRAY_AGG(rs.service_id) FILTER (WHERE rs.service_id IS NOT NULL),
                        '{}'
                    ) AS service_ids
                FROM rooms r
                LEFT JOIN room_services rs
                    ON rs.room_id = r.room_id
                WHERE r.is_active = true
                  AND r.capacity >= @capacity
                  AND NOT EXISTS (
                      SELECT 1
                      FROM bookings b
                      WHERE b.room_id = r.room_id
                        AND b.start_time < @end_time
                        AND b.end_time > @start_time
                        -- тут ще фільтр по активних статусах
                  )
                GROUP BY
                    r.room_id,
                    r.name,
                    r.capacity,
                    r.hourly_rate,
                    r.is_active,
                    r.created_at,
                    r.updated_at
                ORDER BY r.capacity;
                """;

            await using var command = new NpgsqlCommand(sql, conn, tx);

            command.Parameters.AddWithValue("@capacity", capacity);
            command.Parameters.AddWithValue("@start_time", startTime);
            command.Parameters.AddWithValue("@end_time", endTime);

            await using var reader = await command.ExecuteReaderAsync();

            var rooms = new List<Room>();

            while (await reader.ReadAsync())
            {
                var serviceIds = reader.IsDBNull(7) ? new List<long>() : reader.GetFieldValue<long[]>(7).ToList();

                rooms.Add(new Room
                {
                    RoomId = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Capacity = reader.GetInt32(2),
                    HourlyRate = reader.GetDecimal(3),
                    IsActive = reader.GetBoolean(4),
                    CreatedAt = reader.GetDateTime(5),
                    UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    ServiceIds = serviceIds
                });
            }

            return rooms;
        }
    }
}
