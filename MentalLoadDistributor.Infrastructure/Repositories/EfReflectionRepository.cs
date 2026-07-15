using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MentalLoadDistributor.Infrastructure.Repositories
{
    public class EfReflectionRepository
     : IReflectionRepository
    {
        private readonly AppDbContext _context;

        public EfReflectionRepository(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<DailyReflection> AddAsync(
            DailyReflection reflection)
        {
            _context.DailyReflections.Add(
                reflection);

            await _context.SaveChangesAsync();

            return reflection;
        }

        public async Task<DailyReflection?> GetByIdAsync(
            Guid id)
        {
            return await _context
                .DailyReflections
                .Include(r => r.Activities)
                .FirstOrDefaultAsync(
                    r => r.Id == id);
        }

        public async Task<List<DailyReflection>> GetByUserAsync(
            Guid userId)
        {
            return await _context
                .DailyReflections
                .Include(r => r.Activities)
                .Where(r => r.UserId == userId)
                .OrderByDescending(
                    r => r.ReflectionDate)
                .ToListAsync();
        }

        public async Task<List<DailyReflection>> GetTodayByUserAsync(
    Guid userId)
        {
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _context.DailyReflections
                .Include(r => r.Activities)
                .Where(r =>
                    r.UserId == userId &&
                    r.ReflectionDate >= today &&
                    r.ReflectionDate < tomorrow)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}
