using MentalLoadDistributor.Core.Domain.Models.Insights;

namespace MentalLoadDistributor.Core.Interfaces;

public interface IInsightsService
{
    Task<InsightsDto> GetInsightsAsync(Guid familyId, string period);
}