using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models.AI
{
    public class GenerateTaskSuggestionsRequest
    {
        public string Prompt { get; set; } = string.Empty;
    }
}
