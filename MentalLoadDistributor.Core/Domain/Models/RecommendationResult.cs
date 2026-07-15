using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Domain.Models
{
    public class RecommendationResult
    {
        public Guid? UserId { get; set; }

        public string UserName { get; set; } = string.Empty;

        public List<string> Reasons { get; set; } = new();
    }
}
