using MentalLoadDistributor.Core.Domain.Models;

namespace MentalLoadDistributor.Core.Domain.Models.Insights
{
    public class ReflectionInsightsDto
    {
        public bool ShowReflectionList { get; set; }

        public List<ReflectionDto> Reflections { get; set; } = new();

        
    }
}
