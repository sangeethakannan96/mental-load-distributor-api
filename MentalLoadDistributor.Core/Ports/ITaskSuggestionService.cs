using MentalLoadDistributor.Core.Models.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Ports
{
    public interface ITaskSuggestionService
    {
        Task<List<SuggestedTask>>
            GenerateHouseholdSuggestionsAsync(
                string householdDescription);

        Task<List<SuggestedTask>> GenerateTaskSuggestionsAsync(
    string prompt);

    }
}
