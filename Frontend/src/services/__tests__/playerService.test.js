import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../playerService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() } }));

describe('playerService', () => {
  it('listPlayersByUniversity requires id', async () => {
    await expect(svc.listPlayersByUniversity()).rejects.toThrow(/universityId/);
  });
  it('listPlayersByTournament requires id', async () => {
    await expect(svc.listPlayersByTournament()).rejects.toThrow(/tournamentId/);
  });
  it('createPlayer validation', async () => {
    await expect(svc.createPlayer(1, { age: 0 })).rejects.toThrow(/Player name/);
    await expect(svc.createPlayer(1, { name: 'A', age: 200 })).rejects.toThrow(/Age/);
    await expect(svc.createPlayer(1, { name: 'A', gender: 'X' })).rejects.toThrow(/Gender/);
  });
  it('createPlayer success maps body', async () => {
    api.post.mockResolvedValueOnce({ data: { id: 9, name: 'P' } });
    const res = await svc.createPlayer(2, { name: 'P', age: 20, gender: 'Male' });
    expect(api.post).toHaveBeenCalledWith('/api/universities/2/players', { name: 'P', age: 20, gender: 'Male' });
    expect(res.id).toBe(9);
  });
  it('updatePlayer validation', async () => {
    await expect(svc.updatePlayer(1, { age: 10 })).rejects.toThrow(/Player name/);
  });
  it('updatePlayer success', async () => {
    api.put.mockResolvedValueOnce({ data: { ok: true } });
    await svc.updatePlayer(7, { name: 'Z', age: 30, gender: 'Female' });
    expect(api.put).toHaveBeenCalled();
  });
  it('deletePlayer calls endpoint', async () => {
    api.delete.mockResolvedValueOnce({ data: true });
    await svc.deletePlayer(5);
    expect(api.delete).toHaveBeenCalledWith('/api/players/5');
  });
});
