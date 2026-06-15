using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.DTO.Task
{
    public class UpdateTaskDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }

        public TaskPriority Priority { get; set; }

        public int EstimatedMinutes { get; set; }

        public bool IsCompleted { get; set; }
        public Guid? AssignedToId { get; set; }

        public RecurrenceType Recurrence { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}
