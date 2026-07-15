using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models
{
    public class DailyReflection
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public User User { get; set; } = null!;

        public Guid FamilyId { get; set; }

        public Family Family { get; set; } = null!;

        public DateTime ReflectionDate { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public List<ActivityLog> Activities { get; set; } = new();

        public string? Summary { get; set; }
    }
}
