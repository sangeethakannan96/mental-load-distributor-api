using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MentalLoadDistributor.Core.Models;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetByFamilyIdAsync(Guid familyId);
        Task<User?> GetAsync(Guid id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task RemoveAsync(Guid id);
    }
}
