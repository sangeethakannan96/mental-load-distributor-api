using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Models
{
    public class FamilyProfile
    {
        public Guid Id { get; set; }

        public Guid FamilyId { get; set; }

        public Family Family { get; set; }

        public string HouseholdDescription { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
