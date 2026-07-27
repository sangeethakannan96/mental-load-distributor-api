using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class MemberContributionDto
    {
        public Guid UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public int CompletedTasks { get; set; }

        public int ActivitiesCaptured { get; set; }

        public int TotalContributions =>
            CompletedTasks + ActivitiesCaptured;
    }
}
