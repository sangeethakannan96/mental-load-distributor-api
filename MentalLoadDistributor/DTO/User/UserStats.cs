namespace MentalLoadDistributor.DTO.User
{
    public class UserStatsDto
    {
        public int TotalTasks { get; set; }

        public int CompletedTasks { get; set; }

        public int PendingTasks { get; set; }

        public double CompletionRate { get; set; }
    }
}
