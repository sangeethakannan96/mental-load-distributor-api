using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MentalLoadDistributor.Core.Domain.Models;

namespace MentalLoadDistributor.Core.Ports
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllAsync();
        Task<IEnumerable<TaskItem>> GetByFamilyIdAsync(Guid familyId);
        Task<TaskItem?> GetAsync(Guid id);
        Task AddAsync(TaskItem task);
        Task UpdateAsync(TaskItem task);
        Task RemoveAsync(Guid id);

        Task<List<TaskItem>> GetByFamilyIdAsync(
    Guid familyId,
    DateTime startDate,
    DateTime endDate);
    }
}
