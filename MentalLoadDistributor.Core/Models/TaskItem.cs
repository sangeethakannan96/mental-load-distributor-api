using System;
using System.Collections.Generic;

namespace MentalLoadDistributor.Core.Models
{
    public enum TaskPriority
    {
        Low,
        Medium,
        High
    }

    public enum RecurrenceType
    {
        None,
        Daily,
        Weekly,
        Monthly
    }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public Guid CreatedById { get; set; }
        public  User? CreatedBy { get; set; }

        public Guid? AssignedToId { get; set; }
        public User? AssignedTo { get; set; }

        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public bool IsCompleted { get; set; }

        public int EmotionalLoadEstimate { get; set; }
        public int EstimatedMinutes { get; set; }

        public List<string> Tags { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();

        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;
    }
}