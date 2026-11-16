
using MentalLoadDistributor.Core.Domain.Enums;
using MentalLoadDistributor.Core.Domain.Models;

namespace MentalLoadDistributor.API.DTOs
{
    public class UpdateApprovedPlanItemRequest
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public TaskCategory Category { get; set; }
            = TaskCategory.Other;

        public RecurrenceType Recurrence { get; set; }
            = RecurrenceType.None;

        public int EstimatedMinutes { get; set; }

        public string? SuggestedAssigneeRole { get; set; }
    }
}
