using ConferenceRooms.Entities;
using Npgsql;

namespace ConferenceRooms.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        public Task<RefreshToken?> GetActiveRefreshToken(string hash, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task<string> GetRolesUserByUserId(long userId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task<User?> GetUserByEmail(string email, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task InsertRefreshToken(long userId, string refreshTokenHash, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task<int> RevokeRefreshToken(long refreshTokenId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
        public Task<long> UserRegisterAsync(User user, NpgsqlConnection conn, NpgsqlTransaction? tx = null);
    }
}
