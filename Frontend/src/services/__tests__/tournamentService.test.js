import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../tournamentService.js';

vi.mock('../apiClient.js', () => ({ default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() } }));

describe('tournamentService mapping & validation', () => {
  it('maps list response backend -> UI shape', async () => {
    api.get.mockResolvedValueOnce({ data: [{ id: 1, name: 'Cup', venue: 'Pool', date: '2025-01-01', endDate: '2025-01-02' }] });
    const list = await svc.listTournaments();
    expect(list[0]).toEqual({ id: 1, name: 'Cup', location: 'Pool', startDate: '2025-01-01', endDate: '2025-01-02' });
  });

  it('createTournament validates required fields', async () => {
    await expect(svc.createTournament({ name: 'OnlyName' })).rejects.toThrow(/required/);
  });

  it('createTournament posts mapped payload', async () => {
    api.post.mockResolvedValueOnce({ data: { id: 9, name: 'New', venue: 'V', date: '2025-02-01' } });
    const res = await svc.createTournament({ name: 'New', location: 'V', startDate: '2025-02-01' });
    expect(api.post).toHaveBeenCalledWith('/api/tournaments', expect.objectContaining({ name: 'New', venue: 'V', date: '2025-02-01' }));
    expect(res).toMatchObject({ name: 'New', location: 'V' });
  });

  it('updateTournament validates required fields', async () => {
    await expect(svc.updateTournament(1, { name: 'X' })).rejects.toThrow(/required/);
  });

  it('deleteTournament calls correct endpoint', async () => {
    api.delete.mockResolvedValueOnce({ data: true });
    await svc.deleteTournament(5);
    expect(api.delete).toHaveBeenCalledWith('/api/tournaments/5');
  });
});
