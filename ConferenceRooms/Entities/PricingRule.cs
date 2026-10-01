namespace ConferenceRooms.Entities
{
    public class PricingRule
    {
        public long PricingRuleId { get; set; }

        public string Name { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public decimal AdjustmentPercent { get; set; }

        public int Priority { get; set; }

        public bool IsActive { get; set; }
    }
}
