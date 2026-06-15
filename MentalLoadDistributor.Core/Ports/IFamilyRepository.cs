using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IFamilyRepository
    {
        Task<IEnumerable<Family>> GetAllAsync();
        Task<Family?> GetAsync(Guid id);
        Task AddAsync(Family family);
        Task UpdateAsync(Family family);
        Task RemoveAsync(Guid id);

        Task<Family?> GetWithMembersAsync(Guid id);
    }
}
