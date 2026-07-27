using MentalLoadDistributor.Core.Domain.Enums;

namespace MentalLoadDistributor.DTO.Reflection
{
    public class ActivityLogResponse
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ActivityCategory Category { get; set; }

        public int EstimatedMinutes { get; set; }

        public int MentalLoadScore { get; set; }

        public DateTime OccuredAt { get; set; }
    }
}
