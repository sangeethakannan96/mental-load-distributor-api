using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MentalLoadDistributor.Core.Models;
using MentalLoadDistributor.Core.Ports;
using MentalLoadDistributor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MentalLoadDistributor.Infrastructure.Repositories
{
    public class EfFamilyRepository : IFamilyRepository
    {
        private readonly AppDbContext _db;

        public EfFamilyRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Family>> GetAllAsync()
        {
            return await _db.Families
                .Include(f => f.Members)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Family?> GetAsync(Guid id)
        {
            return await _db.Families
                .Include(f => f.Members)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        public async Task AddAsync(Family family)
        {
            await _db.Families.AddAsync(family);
            await _db.SaveChangesAsync();
        }
        public async Task UpdateAsync(Family family)
        {
            _db.Families.Update(family);
            await _db.SaveChangesAsync();
        }
        public async Task RemoveAsync(Guid id)
        {
            var e = await _db.Families.FindAsync(id);
            if (e != null) { _db.Families.Remove(e); await _db.SaveChangesAsync(); }
        }

        public async Task<Family?> GetWithMembersAsync(Guid id)
        {
            return await _db.Families
                .Include(f => f.Members)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

      
    }
}
