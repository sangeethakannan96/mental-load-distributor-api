using MentalLoadDistributor.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IApprovedPlanRepository
    {
        Task<ApprovedPlan?> GetByFamilyIdAsync(
            Guid familyId);

        Task AddAsync(
            ApprovedPlan plan);
    }
}
