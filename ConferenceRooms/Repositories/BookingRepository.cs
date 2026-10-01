using ConferenceRooms.Entities;
using ConferenceRooms.Repositories.Interfaces;
using Npgsql;

namespace ConferenceRooms.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        public async Task<long> CreateBooking(Booking booking, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                INSERT INTO bookings
                (
                    room_id,
                    user_id,
                    status_id,
                    start_time,
                    end_time,
                    base_price,
                    discount,
                    surcharge,
                    total_price
                )
                VALUES
                (
                    @room_id,
                    @user_id,
                    @status_id,
                    @start_time,
                    @end_time,
                    @base_price,
                    @discount,
                    @surcharge,
                    @total_price
                )
                RETURNING booking_id;
                """;

            await using var command = new NpgsqlCommand(sql, conn, tx);

            command.Parameters.AddWithValue("@room_id", booking.RoomId);
            command.Parameters.AddWithValue("@user_id", booking.UserId);
            command.Parameters.AddWithValue("@status_id", booking.StatusId);
            command.Parameters.AddWithValue("@start_time", booking.StartTime);
            command.Parameters.AddWithValue("@end_time", booking.EndTime);
            command.Parameters.AddWithValue("@base_price", booking.BasePrice);
            command.Parameters.AddWithValue("@discount", booking.Discount);
            command.Parameters.AddWithValue("@surcharge", booking.Surcharge);
            command.Parameters.AddWithValue("@total_price", booking.TotalPrice);

            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt64(result);
        }

        public async Task<bool> HasOverlappingBooking(long roomId, DateTime startTime, DateTime endTime, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT EXISTS (
                    SELECT 1
                    FROM bookings b
                    JOIN booking_statuses bs
                        ON bs.status_id = b.status_id
                    WHERE b.room_id = @room_id
                      AND bs.name IN ('Pending', 'Confirmed')
                      AND b.start_time < @end_time
                      AND b.end_time > @start_time
                );
                """;

            await using var command = new NpgsqlCommand(sql, conn, tx);

            command.Parameters.AddWithValue("@room_id", roomId);
            command.Parameters.AddWithValue("@start_time", startTime);
            command.Parameters.AddWithValue("@end_time", endTime);

            return (bool)(await command.ExecuteScalarAsync())!;
        }

        public async Task InsertBookingService(long bookingId, long serviceId, decimal price, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                INSERT INTO booking_services
                (
                    booking_id,
                    service_id,
                    price
                )
                VALUES
                (
                    @booking_id,
                    @service_id,
                    @price
                );
                """;

            await using var command = new NpgsqlCommand(sql, conn, tx);

            command.Parameters.AddWithValue("@booking_id", bookingId);
            command.Parameters.AddWithValue("@service_id", serviceId);
            command.Parameters.AddWithValue("@price", price);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<long>> GetRoomServiceIds(long roomId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT service_id
                FROM room_services
                WHERE room_id = @room_id;
                """;

            await using var command = new NpgsqlCommand(sql, conn, tx);

            command.Parameters.AddWithValue("@room_id", roomId);

            await using var reader = await command.ExecuteReaderAsync();

            var serviceIds = new List<long>();

            while (await reader.ReadAsync())
            {
                serviceIds.Add(reader.GetInt64(0));
            }

            return serviceIds;
        }
    }
}
