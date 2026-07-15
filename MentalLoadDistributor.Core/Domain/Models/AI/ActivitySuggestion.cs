using MentalLoadDistributor.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.AI
{
    public class ActivitySuggestion
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public ActivityCategory Category { get; set; }

        public int EstimatedMinutes { get; set; }

        public int MentalLoadScore { get; set; }
    }
}
