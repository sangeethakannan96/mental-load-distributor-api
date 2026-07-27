using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.Dashboards
{
    public class RecentCompletedTaskDto
    {
        public Guid TaskId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string CompletedBy { get; set; } = string.Empty;

        public DateTime CompletedAt { get; set; }

        public TaskPriority Priority { get; set; }
    }
}
