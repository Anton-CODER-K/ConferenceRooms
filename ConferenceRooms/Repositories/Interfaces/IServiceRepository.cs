using ConferenceRooms.Entities;
using Npgsql;

namespace ConferenceRooms.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<long> CreateService(Service service, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task<Service?> GetServiceById(long serviceId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task<List<Service>> GetServices(NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task<int> UpdateService(Service service, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task<int> DeleteService(long serviceId, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

        Task<List<Service>> GetServicesByIds(List<long> serviceIds, NpgsqlConnection conn, NpgsqlTransaction? tx = null);

    }
}
