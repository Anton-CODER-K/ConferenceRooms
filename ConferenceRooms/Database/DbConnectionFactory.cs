using Npgsql;

namespace ConferenceRooms.Database
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string not found");
        }

        public NpgsqlConnection Create()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
