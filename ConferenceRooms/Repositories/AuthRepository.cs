using ConferenceRooms.Entities;
using ConferenceRooms.Enums;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories.Interfaces;
using Npgsql;
using NpgsqlTypes;
using static System.Net.Mime.MediaTypeNames;

namespace ConferenceRooms.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        // Метод для реєстрації користувача в базі даних. Використовує підключення та транзакцію для виконання SQL-запиту на вставку нового користувача
        public async Task<long> UserRegisterAsync(User user, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                Insert Into Users(email, password_hash, role_id)
                Values(@email, @password_hash, @role_id)
                Returning user_id
            """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = user.Email;
            cmd.Parameters.Add("@password_hash", NpgsqlDbType.Varchar).Value = user.PasswordHash;
            cmd.Parameters.Add("@role_id", NpgsqlDbType.Bigint).Value = user.RoleId;

            try
            {
                var result = await cmd.ExecuteScalarAsync();

                return (long)result!;
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                throw new BusinessException(
                    "This email is already registered.",
                    StatusCodes.Status409Conflict);
            }
        }

        public async Task<User?> GetUserByEmail(string email, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT u.user_id, u.email, u.password_hash, u.role_id
                FROM users u
                WHERE u.email = @email
                Limit 1;
            """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = email;

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32(0),
                    Email = reader.GetString(1),
                    PasswordHash =  reader.GetString(2),
                    RoleId = (Role)reader.GetInt32(3)
                };
            }

            return null;
        }

        public async Task InsertRefreshToken(long userId, string refreshTokenHash, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                INSERT INTO refresh_tokens
                (
                    user_id,
                    token_hash,
                    expires_at
                )
                VALUES
                (
                    @user_id,
                    @token_hash,
                    @expires_at
                );
            """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@user_id", NpgsqlDbType.Bigint).Value = userId;
            cmd.Parameters.Add("@token_hash", NpgsqlDbType.Text).Value = refreshTokenHash;

            // можна б було винести значення у функцію шоб вписувати до скількох днів але думаю шо воно для всіх однакове буде або винести у Config шоб можна було настраювати дні звідти
            var expiresAt = DateTime.UtcNow.AddDays(14);

            cmd.Parameters.Add("@expires_at", NpgsqlDbType.TimestampTz).Value = expiresAt;

            await cmd.ExecuteNonQueryAsync();
        }


        public async Task<RefreshToken?> GetActiveRefreshToken(string hash, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT
                    refresh_token_id,
                    user_id,
                    token_hash,
                    expires_at,
                    created_at,
                    revoked_at
                FROM refresh_tokens
                WHERE token_hash = @token_hash
                  AND revoked_at IS NULL
                  AND expires_at > CURRENT_TIMESTAMP;
            """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@token_hash", NpgsqlDbType.Text).Value = hash;

            await using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new RefreshToken
            {
                RefreshTokenId = reader.GetInt64(0),
                UserId = reader.GetInt64(1),
                TokenHash = reader.GetString(2),
                ExpiresAt = reader.GetDateTime(3),
                CreatedAt = reader.GetDateTime(4),
                RevokedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5)
            };
        }

        public async Task<string> GetRolesUserByUserId(long userId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT r.name
                FROM users u
                INNER JOIN roles r ON r.role_id = u.role_id
                WHERE u.user_id = @user_id;
            """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@user_id", NpgsqlDbType.Bigint).Value = userId;

            var result = await cmd.ExecuteScalarAsync();

            if (result is null || result == DBNull.Value)
                throw new BusinessException("User role not found.", StatusCodes.Status401Unauthorized);

            return result.ToString()!;
        }

        public async Task<int> RevokeRefreshToken(long refreshTokenId, NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                UPDATE refresh_tokens
                SET revoked_at = CURRENT_TIMESTAMP
                WHERE refresh_token_id = @refresh_token_id
                  AND revoked_at IS NULL;
            """;

            await using var cmd = new NpgsqlCommand(sql, conn, tx);

            cmd.Parameters.Add("@refresh_token_id", NpgsqlDbType.Bigint).Value =
                refreshTokenId;

            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
