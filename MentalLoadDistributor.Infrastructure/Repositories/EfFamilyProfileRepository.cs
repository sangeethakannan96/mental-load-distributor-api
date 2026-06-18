using MentalLoadDistributor.Core.Models;
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
    public class EfFamilyProfileRepository
     : IFamilyProfileRepository
    {
        private readonly AppDbContext _db;

        public EfFamilyProfileRepository(
            AppDbContext db)
        {
            _db = db;
        }

        public async Task<FamilyProfile?>
            GetByFamilyIdAsync(Guid familyId)
        {
            return await _db.FamilyProfiles
                .FirstOrDefaultAsync(
                    x => x.FamilyId == familyId);
        }

        public async Task AddAsync(
            FamilyProfile profile)
        {
            _db.FamilyProfiles.Add(profile);

            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            FamilyProfile profile)
        {
            await _db.SaveChangesAsync();
        }
    }
}
