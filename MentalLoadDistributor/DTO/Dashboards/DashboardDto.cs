namespace MentalLoadDistributor.DTO.Dashboards
{
    public class DashboardDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }

        public int UnassignedTasks { get; set; }

        public List<UserLoadDto> LoadPerUser { get; set; } = new();

        public string? MostLoadedUser { get; set; }
        public string? LeastLoadedUser { get; set; }

        public double AverageLoad { get; set; }

       
        public List<string> OverloadedUsers { get; set; } = new();
        public List<string> UnderutilizedUsers { get; set; } = new();

        public double LoadImbalanceScore { get; set; }

        public string? Recommendation { get; set; }

        public int OverdueTasks { get; set; }

        public int TasksDueToday { get; set; }

        public int UrgentTasks { get; set; }
    }
}
