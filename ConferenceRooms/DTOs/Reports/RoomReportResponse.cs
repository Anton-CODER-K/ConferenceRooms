namespace ConferenceRooms.DTOs.Reports
{
    public class RoomReportResponse
    {
        public ReportPeriod Period { get; set; } = new();

        public int TotalBookings { get; set; }

        public decimal TotalRevenue { get; set; }

        public List<RoomReportItem> Rooms { get; set; } = new();
    }
    public class ReportPeriod
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class RoomReportItem
    {
        public long RoomId { get; set; }

        public string RoomName { get; set; } = string.Empty;

        public int BookingCount { get; set; }

        public decimal BookedHours { get; set; }

        public decimal Revenue { get; set; }
    }
}
