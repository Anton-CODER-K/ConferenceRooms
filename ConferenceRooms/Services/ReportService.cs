using ConferenceRooms.Database;
using ConferenceRooms.DTOs.Reports;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories.Interfaces;

namespace ConferenceRooms.Services
{
    public class ReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly DbConnectionFactory _connectionFactory;

        public ReportService(IReportRepository reportRepository, DbConnectionFactory connectionFactory)
        {
            _reportRepository = reportRepository;
            _connectionFactory = connectionFactory;
        }

        public async Task<RoomReportResponse> GetRoomReportAsync(DateTime from, DateTime to)
        {
            if (from >= to)
                throw new BusinessException("The 'from' date must be earlier than the 'to' date.");

            var toExclusive = to.Date.AddDays(1);

            await using var connection = _connectionFactory.Create();
            await connection.OpenAsync();

            var rooms = await _reportRepository.GetRoomReport(from.Date, toExclusive, connection);

            return new RoomReportResponse
            {
                Period = new ReportPeriod
                {
                    From = from.Date,
                    To = to.Date
                },

                TotalBookings = rooms.Sum(x => x.BookingCount),

                TotalRevenue = rooms.Sum(x => x.Revenue),

                Rooms = rooms.Select(x => new RoomReportItem
                {
                    RoomId = x.RoomId,
                    RoomName = x.RoomName,
                    BookingCount = x.BookingCount,
                    BookedHours = x.BookedHours,
                    Revenue = x.Revenue
                }).ToList()
            };
        }
    }
}
