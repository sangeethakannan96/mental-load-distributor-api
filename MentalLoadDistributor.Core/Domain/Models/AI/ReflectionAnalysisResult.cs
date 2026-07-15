using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.AI
{
    public class ReflectionAnalysisResult
    {
        public string Summary { get; set; } = string.Empty;

        public List<ActivitySuggestion> Activities { get; set; } = new();
    }
}
