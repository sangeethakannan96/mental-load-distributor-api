using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Interfaces;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MentalLoadDistributor.Infrastructure.Repositories
{
    public class EfApprovedPlanRepository
        : IApprovedPlanRepository
    {
        private readonly AppDbContext _db;

        public EfApprovedPlanRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ApprovedPlan?> GetByFamilyIdAsync(
            Guid familyId)
        {
            return await _db.ApprovedPlans
                .Include(x => x.Items)
                .FirstOrDefaultAsync(
                    x => x.FamilyId == familyId);
        }

        public async Task AddAsync(
            ApprovedPlan plan)
        {
            _db.ApprovedPlans.Add(plan);

            await _db.SaveChangesAsync();
        }
    }
}