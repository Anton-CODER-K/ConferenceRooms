using ConferenceRooms.Database;
using ConferenceRooms.DTOs.Room;
using ConferenceRooms.DTOs.Service;
using ConferenceRooms.Entities;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories;
using ConferenceRooms.Repositories.Interfaces;

namespace ConferenceRooms.Services
{
    public class RoomService
    {
        private readonly ILogger<RoomService> _logger;
        private readonly IRoomRepository _roomRepo;
        private readonly TransactionExecutor _tx;

        public RoomService(ILogger<RoomService> logger, IRoomRepository roomRepo, TransactionExecutor tx)
        {
            _logger = logger;
            _roomRepo = roomRepo;
            _tx = tx;
        }

        public async Task<long> CreateRoomAsync(CreateRoomRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new BusinessException("Room name is required.");

            if (request.Capacity <= 0)
                throw new BusinessException("Room capacity must be greater than 0.");

            if (request.HourlyRate <= 0)
                throw new BusinessException("Hourly rate must be greater than 0.");

            var room = new Room
            {
                Name = request.Name.Trim(),
                Capacity = request.Capacity,
                HourlyRate = request.HourlyRate,
                IsActive = true
            };

            long roomId = 0;

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                roomId = await _roomRepo.CreateRoom(room, conn, tx);

                if (request.ServiceIds.Count == 0)
                {
                    await _roomRepo.InsertServicesToRoom(request.ServiceIds, roomId, conn, tx);
                }
            });

            _logger.LogInformation("Conference room created. RoomId: {RoomId}, Name: {RoomName}", roomId, room.Name);

            return roomId;
        }

        public async Task UpdateRoomAsync(long roomId, UpdateRoomRequest request)
        {
            if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BusinessException("Room name cannot be empty.");
            }

            if (request.Capacity.HasValue && request.Capacity <= 0)
            {
                throw new BusinessException("Room capacity must be greater than 0.");
            }

            if (request.HourlyRate.HasValue && request.HourlyRate <= 0)
            {
                throw new BusinessException("Hourly rate must be greater than 0.");
            }

            await _tx.ExecuteAsync(async (conn, tx) =>
            {
                var room = await _roomRepo.GetRoomById(roomId, conn, tx);

                if (room is null)
                    throw new BusinessException("Room not found.", StatusCodes.Status404NotFound);

                if (request.Name is not null)
                    room.Name = request.Name.Trim();

                if (request.Capacity.HasValue)
                    room.Capacity = request.Capacity.Value;

                if (request.HourlyRate.HasValue)
                    room.HourlyRate = request.HourlyRate.Value;

                await _roomRepo.UpdateRoom(room, conn, tx);

                if (request.ServiceIds is not null)
                {
                    await _roomRepo.DeleteServicesToRoom(roomId, conn, tx);
                    await _roomRepo.InsertServicesToRoom(request.ServiceIds, roomId, conn, tx);
                }
            });

            _logger.LogInformation("Conference room updated. RoomId: {RoomId}", roomId);
        }

        public async Task DeleteRoomAsync(long roomId)
        {

            await _tx.ExecuteAsync(async (conn) =>
            {
                await _roomRepo.DeleteRoom(roomId, conn);
            });

            _logger.LogInformation("Conference room updated. RoomId: {RoomId}", roomId);
        }

        public async Task<List<RoomResponse>> SearchAvailableRoomsAsync(SearchAvailableRoomsRequest request)
        {
            if (request.StartTime >= request.EndTime)
                throw new BusinessException("Start time must be earlier than end time.");

            if (request.Capacity <= 0)
                throw new BusinessException("Capacity must be greater than zero.");

            var rooms = await _tx.ExecuteAsync(async (conn, tx) =>
            {
                return await _roomRepo.SearchAvailableRooms(request.StartTime, request.EndTime, request.Capacity, conn, tx);
            });

            return rooms.Select(room => new RoomResponse
            {
                RoomId = room.RoomId,
                Name = room.Name,
                Capacity = room.Capacity,
                HourlyRate = room.HourlyRate,
                CreatedAt = room.CreatedAt,
                UpdatedAt = room.UpdatedAt,
                ServiceIds = room.ServiceIds
            }).ToList();
        }
    }
}
