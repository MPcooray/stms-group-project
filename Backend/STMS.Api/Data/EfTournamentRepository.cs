using Microsoft.EntityFrameworkCore;
using STMS.Api.Models;

namespace STMS.Api.Data
{
    public class EfTournamentRepository : ITournamentRepository
    {
        private readonly StmsDbContext _db;
        public EfTournamentRepository(StmsDbContext db) => _db = db;

        public Task<List<Tournament>> GetAllAsync(CancellationToken ct = default) =>
            _db.Tournaments.OrderByDescending(t => t.Id).ToListAsync(ct);

        public Task<Tournament?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _db.Tournaments.FindAsync(new object?[] { id }, ct).AsTask();

        public async Task<Tournament> CreateAsync(Tournament t, CancellationToken ct = default)
        {
            _db.Tournaments.Add(t);
            await _db.SaveChangesAsync(ct);
            return t;
        }

        public async Task<bool> UpdateAsync(Tournament t, CancellationToken ct = default)
        {
            if (!await _db.Tournaments.AnyAsync(x => x.Id == t.Id, ct)) return false;
            _db.Entry(t).State = EntityState.Modified;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
        {
            var t = await _db.Tournaments.FindAsync(new object?[] { id }, ct);
            if (t is null) return false;
            _db.Tournaments.Remove(t);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
