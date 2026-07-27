using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models;

namespace MentalLoadDistributor.DTO.Task
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TaskCategory Category { get; set; } = TaskCategory.Other;

        public int EstimatedMinutes { get; set; }

        public int MentalLoadEstimate { get; set; }

        public DateTime? DueDate { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;

        public List<string> Tags { get; set; } = new();
    }
}
