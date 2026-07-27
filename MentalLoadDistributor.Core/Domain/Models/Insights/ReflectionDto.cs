using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class ReflectionDto
    {
        public Guid Id { get; set; }

        public DateTime ReflectionDate { get; set; }

        public string Content { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public List<ActivityLogDto> Activities { get; set; } = new();
    }
}
