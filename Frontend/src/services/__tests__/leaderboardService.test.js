import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../leaderboardService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn() } }));

describe('leaderboardService', () => {
  it('getLeaderboard calls correct endpoint', async () => {
    api.get.mockResolvedValueOnce({ data: { players: [], universities: [] } });
    const data = await svc.getLeaderboard(9);
    expect(api.get).toHaveBeenCalledWith('/api/leaderboard/9');
    expect(data.players).toEqual([]);
  });
});
