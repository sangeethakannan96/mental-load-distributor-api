namespace MentalLoadDistributor.Core.Models
{
    public class SuggestedTask
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string Recurrence { get; set; }

        public DateTime? StartDate { get; set; }

        public string SuggestedAssigneeRole { get; set; }

        public int EmotionalLoad { get; set; }

        public int EstimatedMinutes { get; set; }

        public TaskPriority Priority { get; set; }
    }
}
