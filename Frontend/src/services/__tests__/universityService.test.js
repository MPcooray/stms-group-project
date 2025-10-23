import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../universityService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() } }));

describe('universityService', () => {
  it('validations', async () => {
    await expect(svc.listUniversitiesByTournament()).rejects.toThrow(/tournamentId/);
    await expect(svc.getUniversityById()).rejects.toThrow(/id/);
    await expect(svc.createUniversity()).rejects.toThrow(/tournamentId/);
    await expect(svc.updateUniversity()).rejects.toThrow(/id/);
    await expect(svc.deleteUniversity()).rejects.toThrow(/id/);
  });
  it('list + create + update + delete', async () => {
    api.get.mockResolvedValueOnce({ data: [] });
    await svc.listUniversitiesByTournament(9);
    expect(api.get).toHaveBeenCalledWith('/api/tournaments/9/universities');
    api.post.mockResolvedValueOnce({ data: { id:1 } });
    await svc.createUniversity(9,{ name:'X' });
    expect(api.post).toHaveBeenCalledWith('/api/tournaments/9/universities', { name:'X' });
    api.put.mockResolvedValueOnce({ data:{ id:1 } });
    await svc.updateUniversity(1,{ name:'Y' });
    api.delete.mockResolvedValueOnce({ data:true });
    await svc.deleteUniversity(1);
  });
});
