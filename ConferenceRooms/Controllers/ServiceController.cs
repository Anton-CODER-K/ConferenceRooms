using ConferenceRooms.DTOs.Service;
using ConferenceRooms.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Controllers
{
    [ApiController]
    [Route("/services")]
    public class ServiceController : ControllerBase
    {
        private readonly ServiceCatalog _serviceCatalog;

        public ServiceController(ServiceCatalog serviceCatalog)
        {
            _serviceCatalog = serviceCatalog;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateService([FromBody] CreateServiceRequest request)
        {
            var serviceId = await _serviceCatalog.CreateServiceAsync(request);

            return StatusCode(StatusCodes.Status201Created,
                new{
                    serviceId
                });
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceResponse>>>GetServices()
        {
            var services = await _serviceCatalog.GetServicesAsync();

            return Ok(services);
        }

        [HttpGet("{serviceId}")]
        public async Task<ActionResult<ServiceResponse>> GetService(long serviceId)
        {
            var service = await _serviceCatalog.GetServiceByIdAsync(serviceId);

            return Ok(service);
        }

        [HttpPut("{serviceId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(long serviceId, [FromBody] UpdateServiceRequest request)
        {
            await _serviceCatalog.UpdateServiceAsync(serviceId, request);

            return NoContent();
        }

        [HttpDelete("{serviceId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(long serviceId)
        {
            await _serviceCatalog.DeleteServiceAsync(serviceId);

            return NoContent();
        }
    }
}
