using MentalLoadDistributor.Core.Domain.Models.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(Guid userId);
    }
}
