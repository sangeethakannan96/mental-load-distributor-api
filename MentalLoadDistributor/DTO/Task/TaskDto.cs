using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.DTO.User;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;

namespace MentalLoadDistributor.DTO.Task
{
    public class TaskDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int EstimatedMinutes { get; set; }

        public int MentalLoadEstimate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TaskPriority Priority { get; set; }

        public TaskStatus Status { get; set; }

        public TaskCategory Category { get; set; }

        public UserDto? CreatedBy { get; set; }

        public UserDto? AssignedTo { get; set; }

        public RecurrenceType Recurrence { get; set; }

        public List<string> Tags { get; set; } = new();
    }
}
