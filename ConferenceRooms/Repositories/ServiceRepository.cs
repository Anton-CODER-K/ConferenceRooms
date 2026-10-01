using ConferenceRooms.Entities;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace ConferenceRooms.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        public async Task<long> CreateService(Service service, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                INSERT INTO services
                (
                    name,
                    price,
                    is_active
                )
                VALUES
                (
                    @name,
                    @price,
                    @is_active
                )
                RETURNING service_id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@name", NpgsqlDbType.Text).Value = service.Name;
            cmd.Parameters.Add("@price", NpgsqlDbType.Numeric).Value = service.Price;
            cmd.Parameters.Add("@is_active", NpgsqlDbType.Boolean).Value = true;

            var result = await cmd.ExecuteScalarAsync();

            if (result is null || result == DBNull.Value)
                throw new BusinessException("Failed to create service.");

            return Convert.ToInt64(result);
        }

        public async Task<int> DeleteService(long serviceId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                UPDATE services
                SET
                    is_active = false,
                    updated_at = CURRENT_TIMESTAMP
                WHERE service_id = @service_id
                  AND is_active = true;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@service_id", NpgsqlDbType.Bigint).Value = serviceId;

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<Service?> GetServiceById(long serviceId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT
                    service_id,
                    name,
                    price,
                    is_active,
                    created_at,
                    updated_at
                FROM services
                WHERE service_id = @service_id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@service_id", NpgsqlDbType.Bigint).Value = serviceId;

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new Service
            {
                ServiceId = reader.GetInt64(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                IsActive = reader.GetBoolean(3),
                CreatedAt = reader.GetDateTime(4),
                UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            };
        }

        public async Task<List<Service>> GetServices(NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT
                    service_id,
                    name,
                    price,
                    is_active,
                    created_at,
                    updated_at
                FROM services
                WHERE is_active = true
                ORDER BY service_id;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            await using var reader = await cmd.ExecuteReaderAsync();

            var services = new List<Service>();

            while (await reader.ReadAsync())
            {
                services.Add(new Service
                {
                    ServiceId = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
                });
            }

            return services;
        }

        public async Task<int> UpdateService(Service service, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                UPDATE services
                SET
                    name = @name,
                    price = @price,
                    updated_at = CURRENT_TIMESTAMP
                WHERE service_id = @service_id
                  AND is_active = true;
                """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@service_id", NpgsqlDbType.Bigint).Value = service.ServiceId;
            cmd.Parameters.Add("@name", NpgsqlDbType.Text).Value = service.Name;
            cmd.Parameters.Add("@price", NpgsqlDbType.Numeric).Value = service.Price;

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Service>> GetServicesByIds(List<long> serviceIds, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT
                    service_id,
                    name,
                    price,
                    is_active,
                    created_at,
                    updated_at
                FROM services
                WHERE service_id = ANY(@service_ids)
                  AND is_active = true;
                """;

            await using var command = new NpgsqlCommand(sql, conn, tx);

            command.Parameters.AddWithValue("@service_ids", serviceIds.ToArray());

            await using var reader = await command.ExecuteReaderAsync();

            var services = new List<Service>();

            while (await reader.ReadAsync())
            {
                services.Add(new Service
                {
                    ServiceId = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    IsActive = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4),
                    UpdatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                });
            }

            return services;
        }
    }
}
