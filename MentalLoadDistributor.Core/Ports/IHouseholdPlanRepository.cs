using MentalLoadDistributor.Core.Domain.Models;

namespace MentalLoadDistributor.Core.Interfaces
{
    public interface IHouseholdPlanRepository
    {
        Task<HouseholdPlan?> GetByFamilyIdAsync(
            Guid familyId);

        Task AddAsync(
            HouseholdPlan plan);

        Task UpdateAsync(
            HouseholdPlan plan);
    }
}