namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class ReflectionSummaryDto
    {
        public int TotalReflections { get; set; }

        public int TotalActivitiesCaptured { get; set; }

        public int TotalEstimatedMinutes { get; set; }

        public int MentalLoadScore { get; set; }

        public int InvisibleWorkPercentage { get; set; }

        public List<ActivityCategorySummaryDto> TopCategories { get; set; } = new();

        public string AiSummary { get; set; } = string.Empty;
    }
}
