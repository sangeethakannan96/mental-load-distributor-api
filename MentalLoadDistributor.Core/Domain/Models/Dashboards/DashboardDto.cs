using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.Dashboards
{
    public class DashboardDto
    {
        public DashboardSummaryDto Summary { get; set; } = new();

        public List<MemberTodayDto> FamilyProgress { get; set; } = new();

        public List<RecentCompletedTaskDto> RecentCompletedTasks { get; set; } = new();
    }
}
