using ConferenceRooms.Database;
using ConferenceRooms.DTOs.Service;
using ConferenceRooms.Entities;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories;
using ConferenceRooms.Repositories.Interfaces;
using System.Transactions;

namespace ConferenceRooms.Services
{
    public class ServiceCatalog
    {
        private readonly ILogger<ServiceCatalog> _logger;
        private readonly IServiceRepository _serviceRepo;
        private readonly TransactionExecutor _tx;

        public ServiceCatalog(ILogger<ServiceCatalog> logger, IServiceRepository serviceRepo, TransactionExecutor tx)
        {
            _logger = logger;
            _serviceRepo = serviceRepo;
            _tx = tx;
        }

        public async Task<long> CreateServiceAsync(CreateServiceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BusinessException("Service name is required.");

            if (request.Price <= 0)
                throw new BusinessException("Service price must be greater than 0.");

            var service = new Service
            {
                Name = request.Name.Trim(),
                Price = request.Price,
                IsActive = true
            };

            long serviceId = 0;

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                serviceId = await _serviceRepo.CreateService(service, conn, tx);
            });

            _logger.LogInformation("Service created. ServiceId: {ServiceId}", serviceId);

            return serviceId;
        }

        public async Task<ServiceResponse> GetServiceByIdAsync(long serviceId)
        {
            if (serviceId <= 0)
                throw new BusinessException("Invalid service ID.");

            Service? service = null;

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                service = await _serviceRepo.GetServiceById(serviceId, conn, tx);
            });

            if (service is null)
                throw new BusinessException("Service not found.");

            return new ServiceResponse
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            };
        }

        public async Task<List<ServiceResponse>> GetServicesAsync()
        {
            List<Service> services = new();

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                services = await _serviceRepo.GetServices(conn, tx);
            });

            return services.Select(service => new ServiceResponse
            {
                ServiceId = service.ServiceId,
                Name = service.Name,
                Price = service.Price,
                IsActive = service.IsActive,
                CreatedAt = service.CreatedAt,
                UpdatedAt = service.UpdatedAt
            }).ToList();


        }

        public async Task UpdateServiceAsync(long serviceId, UpdateServiceRequest request)
        {
            if (serviceId <= 0)
                throw new BusinessException(
                    "Invalid service ID.");

            if (request.Name is not null &&
                string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BusinessException(
                    "Service name cannot be empty.");
            }

            if (request.Price.HasValue &&
                request.Price <= 0)
            {
                throw new BusinessException(
                    "Service price must be greater than 0.");
            }

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                var service = await _serviceRepo.GetServiceById(serviceId, conn, tx);

                if (service is null)
                    throw new BusinessException("Service not found.");

                if (!service.IsActive)
                    throw new BusinessException("Service is inactive.");

                if (request.Name is not null)
                    service.Name = request.Name.Trim();

                if (request.Price.HasValue)
                    service.Price = request.Price.Value;
                 
                var affectedRows = await _serviceRepo.UpdateService(service, conn, tx);

                if (affectedRows == 0)
                    throw new BusinessException("Service not found.");
            });

            _logger.LogInformation("Service updated. ServiceId: {ServiceId}", serviceId);
        }

        public async Task DeleteServiceAsync(long serviceId)
        {
            if (serviceId <= 0)
                throw new BusinessException("Invalid service ID.");

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                var affectedRows = await _serviceRepo.DeleteService(serviceId, conn, tx);

                if (affectedRows == 0)
                    throw new BusinessException("Service not found.");
            });

            _logger.LogInformation("Service deleted. ServiceId: {ServiceId}", serviceId);
        }
    }
}
