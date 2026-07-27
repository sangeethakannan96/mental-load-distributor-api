namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class OverviewInsightsDto
    {
        public int TasksCompleted { get; set; }

        public int ActivitiesCaptured { get; set; }

        public int TotalContributions { get; set; }

        public int TotalMinutes { get; set; }

        public int MentalLoadScore { get; set; }

        public List<MemberContributionDto> DivisionOfWork { get; set; } = new();

        public string AIInsight { get; set; } = string.Empty;
    }
}
