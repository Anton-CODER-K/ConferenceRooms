using ConferenceRooms.Entities;
using Npgsql;

namespace ConferenceRooms.Repositories.Interfaces
{
    public interface IPricingRepository
    {
        Task<List<PricingRule>> GetActiveRules(NpgsqlConnection conn, NpgsqlTransaction? tx = null);
    }
}
