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
    public class EfUserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public EfUserRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _db.Users
                .Include(u => u.Family)   // ✅ add this
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetByFamilyIdAsync(Guid familyId)
        {
            return await _db.Users
                .Where(u => u.FamilyId == familyId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<User?> GetAsync(Guid id)
        {
            return await _db.Users
                .Include(u => u.Family)   // ✅ important
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }
        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
        public async Task RemoveAsync(Guid id)
        {
            var e = await _db.Users.FindAsync(id);
            if (e != null) { _db.Users.Remove(e); await _db.SaveChangesAsync(); }
        }
    }
}
