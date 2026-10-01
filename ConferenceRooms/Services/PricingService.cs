using ConferenceRooms.DTOs.Booking;
using ConferenceRooms.Entities;
using ConferenceRooms.Exceptions;
using ConferenceRooms.Repositories.Interfaces;
using ConferenceRooms.Services.Interfaces;
using Npgsql;

namespace ConferenceRooms.Services
{
    public class PricingService : IPricingService
    {
        private readonly IPricingRepository _pricingRepository;

        public PricingService(IPricingRepository pricingRepository)
        {
            _pricingRepository = pricingRepository;
        }

        public async Task<PricingResult> Calculate(
            decimal hourlyRate,
            DateTime startTime,
            DateTime endTime,
            NpgsqlConnection conn,
            NpgsqlTransaction? tx = null)
        {
            var rules = await _pricingRepository.GetActiveRules(conn, tx);

            if (rules.Count == 0)
            {
                throw new BusinessException("No pricing rules configured.");
            }

            decimal basePrice = 0;
            decimal discount = 0;
            decimal surcharge = 0;

            var current = startTime;

            while (current < endTime)
            {
                var rule = GetRuleForTime(current.TimeOfDay, rules);

                var nextBoundary = GetNextBoundary(current, endTime, rules);

                var hours =
                    (decimal)(nextBoundary - current).TotalHours;

                var segmentPrice = hourlyRate * hours;

                basePrice += segmentPrice;

                if (rule.AdjustmentPercent < 0)
                {
                    discount += segmentPrice * Math.Abs(rule.AdjustmentPercent);
                }
                else if (rule.AdjustmentPercent > 0)
                {
                    surcharge += segmentPrice * rule.AdjustmentPercent;
                }

                current = nextBoundary;
            }

            return new PricingResult
            {
                BasePrice = basePrice,
                Discount = discount,
                Surcharge = surcharge,
                TotalPrice = basePrice - discount + surcharge
            };
        }

        private PricingRule GetRuleForTime(TimeSpan time, List<PricingRule> rules)
        {
            var matchingRules = rules
                .Where(rule =>
                    time >= rule.StartTime &&
                    time < rule.EndTime)
                .OrderByDescending(rule => rule.Priority)
                .ToList();

            if (matchingRules.Count == 0)
            {
                throw new BusinessException($"No pricing rule found for {time}.");
            }

            return matchingRules[0];
        }

        private DateTime GetNextBoundary(DateTime current, DateTime endTime, List<PricingRule> rules)
        {
            var date = current.Date;

            var boundaries = rules
                .SelectMany(rule => new[]
                {
                    rule.StartTime,
                    rule.EndTime
                })
                .Distinct()
                .Select(time => date.Add(time))
                .Where(time => time > current)
                .OrderBy(time => time)
                .ToList();

            var nextBoundary = boundaries.FirstOrDefault();

            if (nextBoundary == default || nextBoundary > endTime)
            {
                return endTime;
            }

            return nextBoundary;
        }
    }
}
