using ConferenceRooms.Database;
using ConferenceRooms.DTOs.Booking;
using ConferenceRooms.Entities;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories;
using ConferenceRooms.Repositories.Interfaces;
using ConferenceRooms.Services.Interfaces;

namespace ConferenceRooms.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IPricingService _pricingService;
        private readonly TransactionExecutor _tx;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepo,
            IRoomRepository roomRepo,
            IServiceRepository serviceRepo,
            IPricingService pricingService,
            TransactionExecutor tx,
            ILogger<BookingService> logger)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _serviceRepo = serviceRepo;
            _pricingService = pricingService;
            _tx = tx;
            _logger = logger;
        }

        public async Task<BookingResponse> CreateBookingAsync(long userId, CreateBookingRequest request)
        {
            ValidateRequest(request);

            var result = await _tx.ExecuteAsync(async (conn, tx) =>
            {
                var room = await _roomRepo.GetRoomById(request.RoomId, conn, tx);

                if (room == null)
                    throw new BusinessException("Room not found.");

                if (!room.IsActive)
                    throw new BusinessException("Room is not active.");
               
                var endTime = request.StartTime.AddMinutes(request.DurationMinutes);
               
                var isBooked = await _bookingRepo.HasOverlappingBooking(request.RoomId, request.StartTime, endTime, conn, tx);

                if (isBooked)
                    throw new BusinessException("Room is already booked for this time.");
               
                var services = await _serviceRepo.GetServicesByIds(request.ServiceIds, conn, tx);

                var roomServiceIds = await _bookingRepo.GetRoomServiceIds(request.RoomId, conn, tx);

                var invalidServices = request.ServiceIds
                    .Except(roomServiceIds)
                    .ToList();

                if (invalidServices.Count > 0)
                {
                    throw new BusinessException(
                        "One or more selected services are not available for this room.");
                }
                if (services.Count != request.ServiceIds.Count)
                    throw new BusinessException("One or more services do not exist.");
               
                var pricing = await _pricingService.Calculate(room.HourlyRate, request.StartTime, endTime, conn, tx);
            
                var servicesPrice = services.Sum(x => x.Price);

                var totalPrice = pricing.TotalPrice + servicesPrice;

                var booking = new Booking
                {
                    RoomId = room.RoomId,
                    UserId = userId,
                    StatusId = 1, // Pending
                    StartTime = request.StartTime,
                    EndTime = endTime,

                    BasePrice = pricing.BasePrice,
                    Discount = pricing.Discount,
                    Surcharge = pricing.Surcharge,
                    TotalPrice = totalPrice
                };

                var bookingId = await _bookingRepo.CreateBooking(booking, conn, tx);

                foreach (var service in services)
                {
                    await _bookingRepo.InsertBookingService(bookingId, service.ServiceId, service.Price, conn, tx);
                }

                return new BookingResponse
                {
                    BookingId = bookingId,
                    RoomId = booking.RoomId,
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                    BasePrice = booking.BasePrice,
                    Discount = booking.Discount,
                    Surcharge = booking.Surcharge,
                    TotalPrice = booking.TotalPrice,
                    Status = "Pending",
                    ServiceIds = services
                        .Select(x => x.ServiceId)
                        .ToList()
                };
            });

            _logger.LogInformation("Booking {BookingId} created by user {UserId}", result.BookingId, userId);

            return result;
        }


        private static void ValidateRequest(CreateBookingRequest request)
        {
            if (request.RoomId <= 0)
                throw new BusinessException("Invalid room ID.");

            if (request.DurationMinutes < 60)
                throw new BusinessException(
                    "Minimum booking duration is 1 hour.");

            if (request.DurationMinutes % 60 != 0)
                throw new BusinessException(
                    "Booking duration must be a whole number of hours.");

            if (request.ServiceIds.Any(x => x <= 0))
                throw new BusinessException(
                    "Invalid service ID.");

            if (request.StartTime < DateTime.UtcNow)
                throw new BusinessException(
                    "Booking start time cannot be in the past.");
        }
    }
}
