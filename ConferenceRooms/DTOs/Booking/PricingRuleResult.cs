using ConferenceRooms.Enums;

namespace ConferenceRooms.DTOs.Booking
{
    public class PricingRuleResult
    {
        public PricingRuleType Type { get; set; }

        public decimal Percent { get; set; }
    }
}
