using ConferenceRooms.Entities;
using ConferenceRooms.Repositories.Interfaces;
using Npgsql;

namespace ConferenceRooms.Repositories
{
    public class PricingRepository : IPricingRepository
    {
        public async Task<List<PricingRule>> GetActiveRules(NpgsqlConnection conn, NpgsqlTransaction? tx = null)
        {
            const string sql = """
                SELECT
                    pricing_rule_id,
                    name,
                    start_time,
                    end_time,
                    adjustment_percent,
                    priority,
                    is_active
                FROM pricing_rules
                WHERE is_active = true
                ORDER BY priority DESC;
                """;

            await using var command = new NpgsqlCommand(sql, conn, tx);

            await using var reader = await command.ExecuteReaderAsync();

            var rules = new List<PricingRule>();

            while (await reader.ReadAsync())
            {
                rules.Add(new PricingRule
                {
                    PricingRuleId = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    StartTime = reader.GetFieldValue<TimeSpan>(2),
                    EndTime = reader.GetFieldValue<TimeSpan>(3),
                    AdjustmentPercent = reader.GetDecimal(4),
                    Priority = reader.GetInt32(5),
                    IsActive = reader.GetBoolean(6)
                });
            }

            return rules;
        }
    }
}
