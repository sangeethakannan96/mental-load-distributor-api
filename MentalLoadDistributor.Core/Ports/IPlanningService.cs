using MentalLoadDistributor.Core.Domain.Models.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IPlanningService
    {
        Task<List<SuggestedTask>> GenerateHouseholdPlanAsync(
                string householdDescription, string householdPlan);

        Task<List<SuggestedTask>> GenerateEventPlanAsync(
    string prompt);

        Task<List<SuggestedTask>> GenerateDailyPlanAsync(
    string prompt);

        Task<List<SuggestedTask>> GenerateWeeklyPlanAsync(
    string prompt);

        Task<List<SuggestedTask>> GenerateMonthlyPlanAsync(
    string prompt);



    }
}
