namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class TaskInsightsDto
    {
        public int TotalTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int PendingTasks { get; set; }

        public int CancelledTasks { get; set; }

        public int OverdueTasks { get; set; }
    }
}
