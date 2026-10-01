namespace ConferenceRooms.DTOs.Booking
{
    public class PricingResult
    {
        public decimal BasePrice { get; set; }

        public decimal Discount { get; set; }

        public decimal Surcharge { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
