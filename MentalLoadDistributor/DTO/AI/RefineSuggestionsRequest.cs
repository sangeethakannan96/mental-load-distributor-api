using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Domain.Models.AI;

namespace MentalLoadDistributor.DTOs
{
    public class RefineSuggestionsRequest
    {
        public List<SuggestedTask> Suggestions { get; set; } = new();

        public string? RefinementInstructions { get; set; }
    }
}