using MentalLoadDistributor.Core.Domain.Models;

namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class InsightsDto
    {
        public OverviewInsightsDto Overview { get; set; } = new();

        public TaskInsightsDto Tasks { get; set; } = new();

        public List<MemberInsightsDto> Family { get; set; } = new();

        public ReflectionInsightsDto Reflections { get; set; } = new();
        public ReflectionSummaryDto? ReflectionSummary { get; set; }
    }
}
