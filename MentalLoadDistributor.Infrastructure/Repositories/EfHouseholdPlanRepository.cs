using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Interfaces;
using MentalLoadDistributor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MentalLoadDistributor.Infrastructure.Repositories
{
    public class EfHouseholdPlanRepository
        : IHouseholdPlanRepository
    {
        private readonly AppDbContext _db;

        public EfHouseholdPlanRepository(
            AppDbContext db)
        {
            _db = db;
        }

        public async Task<HouseholdPlan?> GetByFamilyIdAsync(
            Guid familyId)
        {
            return await _db.HouseholdPlans
                .FirstOrDefaultAsync(
                    x => x.FamilyId == familyId);
        }

        public async Task AddAsync(
            HouseholdPlan plan)
        {
            _db.HouseholdPlans.Add(plan);

            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            HouseholdPlan plan)
        {
            await _db.SaveChangesAsync();
        }
    }
}