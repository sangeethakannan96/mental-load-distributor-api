
namespace MentalLoadDistributor.DTOs
{
    public class ApprovedPlanResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<ApprovedPlanItemResponseDto> Items { get; set; }
            = new();
    }

    public class ApprovedPlanItemResponseDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? Recurrence { get; set; }

        public DateTime? StartDate { get; set; }

        public string? SuggestedAssigneeRole { get; set; }

        public int EstimatedMinutes { get; set; }

        public int MentalLoadEstimate { get; set; }
    }
}
