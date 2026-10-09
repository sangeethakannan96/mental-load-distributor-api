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

        public async Task<ApprovedPlan?> GetByIdWithTasksAsync(
    Guid planId)
        {
            return await _db.ApprovedPlans
                .Include(p => p.Items)
                    .ThenInclude(item => item.Tasks)
                .FirstOrDefaultAsync(p => p.Id == planId);
        }


       

        

public async Task UpdateAsync(
    ApprovedPlan plan,
    IReadOnlyCollection<ApprovedPlanItem> newItems)
        {
            foreach (var item in newItems)
            {
                _db.Entry(item).State = EntityState.Added;
            }

            await _db.SaveChangesAsync();
        }


        public async Task CheckPlanItemsAsync(ApprovedPlan plan)
        {
            foreach (var item in plan.Items)
            {
                var exists = await _db.ApprovedPlanItems
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == item.Id);

                Console.WriteLine(
                    $"PlanItemId={item.Id}, ExistsInDatabase={exists}");
            }


        }

    }
}