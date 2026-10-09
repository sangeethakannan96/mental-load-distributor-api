using MentalLoadDistributor.Core.Domain.Enums;

namespace MentalLoadDistributor.Core.Domain.Models
{
    public class ApprovedPlanItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ApprovedPlanId { get; set; }

        public ApprovedPlan ApprovedPlan { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public Guid? AssignedToId { get; set; }

        public User? AssignedTo { get; set; }

        public TaskPriority Priority { get; set; }
            = TaskPriority.Medium;

        public TaskCategory Category { get; set; }
            = TaskCategory.Other;

        public DateTime? StartDate { get; set; }

        public string? SuggestedAssigneeRole { get; set; }

        public int EstimatedMinutes { get; set; }

        public int MentalLoadEstimate { get; set; }

        public RecurrenceType Recurrence { get; set; }
            = RecurrenceType.None;

        public List<string> Tags { get; set; }
            = new();
    }
}