namespace MentalLoadDistributor.Core.Domain.Models.Insights

{
    public class ActivityCategorySummaryDto
    {
        public string Category { get; set; } = string.Empty;

        public int Count { get; set; }

        public int EstimatedMinutes { get; set; }
    }
}
