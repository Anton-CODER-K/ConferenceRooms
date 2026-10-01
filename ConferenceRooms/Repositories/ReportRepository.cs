using ConferenceRooms.Entities;
using ConferenceRooms.Repositories.Interfaces;
using Npgsql;

namespace ConferenceRooms.Repositories
{
    public class ReportRepository : IReportRepository
    {
        public async Task<List<RoomReport>> GetRoomReport(DateTime from, DateTime to, NpgsqlConnection connection, NpgsqlTransaction? transaction = null)
        {
            const string sql = """
                SELECT
                    r.room_id,
                    r.name AS room_name,
                    COUNT(b.booking_id) AS booking_count,
                    COALESCE(
                        SUM(EXTRACT(EPOCH FROM (b.end_time - b.start_time)) / 3600),
                        0
                    ) AS booked_hours,
                    COALESCE(
                        SUM(b.total_price),
                        0
                    ) AS revenue
                FROM rooms r
                LEFT JOIN bookings b
                    ON b.room_id = r.room_id
                    AND b.start_time >= @from
                    AND b.start_time < @to
                    AND b.status_id IN (
                        SELECT status_id
                        FROM booking_statuses
                        WHERE name IN ('Pending', 'Confirmed', 'Completed')
                    )
                WHERE r.is_active = true
                GROUP BY r.room_id, r.name
                ORDER BY r.room_id;
                """;

            await using var command = new NpgsqlCommand(sql, connection, transaction);

            command.Parameters.AddWithValue("@from", from);
            command.Parameters.AddWithValue("@to", to);

            await using var reader = await command.ExecuteReaderAsync();

            var result = new List<RoomReport>();

            while (await reader.ReadAsync())
            {
                result.Add(new RoomReport
                {
                    RoomId = reader.GetInt64(0),
                    RoomName = reader.GetString(1),
                    BookingCount = Convert.ToInt32(reader.GetInt64(2)),
                    BookedHours = reader.GetDecimal(3),
                    Revenue = reader.GetDecimal(4)
                });
            }

            return result;
        }
    }
}
