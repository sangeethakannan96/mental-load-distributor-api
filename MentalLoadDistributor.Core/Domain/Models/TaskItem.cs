using MentalLoadDistributor.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using TaskStatus = MentalLoadDistributor.Core.Domain.Enums.TaskStatus;



namespace MentalLoadDistributor.Core.Domain.Models
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

        public Guid FamilyId { get; set; }

        public Family Family { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid CreatedById { get; set; }

        public User? CreatedBy { get; set; }

        public Guid? AssignedToId { get; set; }

        public User? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        public DateTime? CompletedAt { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public TaskCategory Category { get; set; } = TaskCategory.Other;

        public int EstimatedMinutes { get; set; }

        public int MentalLoadEstimate { get; set; }

        public RecurrenceType Recurrence { get; set; } = RecurrenceType.None;

        public List<string> Tags { get; set; } = new();

        public Dictionary<string, string> Metadata { get; set; } = new();
    }
}