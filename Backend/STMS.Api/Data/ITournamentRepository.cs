using STMS.Api.Models;

namespace STMS.Api.Data
{
    public interface ITournamentRepository
    {
        Task<List<Tournament>> GetAllAsync(CancellationToken ct = default);
        Task<Tournament?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Tournament> CreateAsync(Tournament t, CancellationToken ct = default);
        Task<bool> UpdateAsync(Tournament t, CancellationToken ct = default);
        Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    }
}
