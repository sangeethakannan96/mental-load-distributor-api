using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Models
{
    public class ApproveSuggestionsRequest
    {
        public List<SuggestedTask> Suggestions { get; set; } = new();
    }
}
