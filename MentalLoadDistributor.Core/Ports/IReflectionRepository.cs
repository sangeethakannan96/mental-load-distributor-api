using MentalLoadDistributor.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Core.Ports
{
    public interface IReflectionRepository
    {
        Task<DailyReflection> AddAsync(
            DailyReflection reflection);

        Task<DailyReflection?> GetByIdAsync(
            Guid id);

        Task<List<DailyReflection>> GetByUserAsync(
            Guid userId);

        Task<List<DailyReflection>> GetTodayByUserAsync(Guid userId);
    }
}
