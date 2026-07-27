namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class MemberInsightsDto
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int CompletedTasks { get; set; }

        public int ActivitiesCaptured { get; set; }

        public int TotalContributions =>
            CompletedTasks + ActivitiesCaptured;
    }
}
