using MentalLoadDistributor.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IFamilyProfileRepository
    {
        Task<FamilyProfile?> GetByFamilyIdAsync(Guid familyId);

        Task AddAsync(FamilyProfile profile);

        Task UpdateAsync(FamilyProfile profile);
    }
}
