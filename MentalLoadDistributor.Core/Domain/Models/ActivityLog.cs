using MentalLoadDistributor.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models
{
    public class ActivityLog
    {
        public Guid Id { get; set; }

        public Guid DailyReflectionId { get; set; }

        public DailyReflection DailyReflection { get; set; } = null!;

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ActivityCategory Category { get; set; }

        public int EstimatedMinutes { get; set; }

        public int MentalLoadScore { get; set; }

        public bool WasPlanned { get; set; }

        public DateTime OccurredAt { get; set; }
    }
}
