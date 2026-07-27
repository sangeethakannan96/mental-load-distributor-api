using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class ActivityLogDto
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public int EstimatedMinutes { get; set; }

        public DateTime ActivityDate { get; set; }

        public int MentalLoadScore { get; set; }
    }
}
