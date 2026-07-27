namespace MentalLoadDistributor.Core.Domain.Models.Dashboards
{
    public class DashboardSummaryDto
    {
        // Task Summary
        public int TotalTasks { get; set; }

        public int PendingTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int UnassignedTasks { get; set; }


    }
}
