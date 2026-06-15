using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.DTO.Task
{
    public class CreateTaskDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public int EstimatedMinutes { get; set; }
        public int EmotionalLoadEstimate { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }

        public RecurrenceType Recurrence { get; set; }

    }
}
