using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MentalLoadDistributor.Core.Domain.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MentalLoadDistributor.Infrastructure.Repositories
{
    public class EfTaskRepository : ITaskRepository
    {
        private readonly AppDbContext _db;

        public EfTaskRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _db.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<TaskItem?> GetAsync(Guid id)
        {
            return await _db.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)   // ✅ add this
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<TaskItem>> GetByFamilyIdAsync(Guid familyId)
        {
            return await _db.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t => t.CreatedBy.FamilyId == familyId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(TaskItem task)
        {
            await _db.Tasks.AddAsync(task);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(TaskItem task)
        {
            _db.Entry(task).State = EntityState.Modified;

            await _db.SaveChangesAsync();
        }

        public async Task RemoveAsync(Guid id)
        {
            var e = await _db.Tasks.FindAsync(id);
            if (e != null) { _db.Tasks.Remove(e); await _db.SaveChangesAsync(); }
        }

        public async Task<List<TaskItem>> GetByFamilyIdAsync(
    Guid familyId,
    DateTime startDate,
    DateTime endDate)
        {
            return await _db.Tasks
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .Where(t =>
                    t.CreatedBy != null &&
                    t.CreatedBy.FamilyId == familyId &&
                    t.DueDate.HasValue &&
                    t.DueDate.Value.Date >= startDate.Date &&
                    t.DueDate.Value.Date <= endDate.Date)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }
    }
}
