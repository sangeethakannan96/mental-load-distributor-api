using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.DTO.Task
{
    public class UpdateTaskDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TaskCategory Category { get; set; }

        public DateTime? DueDate { get; set; }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        public int EstimatedMinutes { get; set; }

        public int MentalLoadEstimate { get; set; }

        public Guid? AssignedToId { get; set; }

        public RecurrenceType Recurrence { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}
