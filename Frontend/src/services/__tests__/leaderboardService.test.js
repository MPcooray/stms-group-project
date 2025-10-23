import { describe, expect, it, vi } from 'vitest';
// Ensure the apiClient mock is registered before importing the service module
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn() } }));
import api from '../apiClient.js';
import * as svc from '../leaderboardService.js';

describe('leaderboardService', () => {
  it('getLeaderboard calls correct endpoint', async () => {
    api.get.mockResolvedValueOnce({ data: { players: [], universities: [] } });
    const data = await svc.getLeaderboard(9);
  expect(api.get).toHaveBeenCalledWith('/api/leaderboard/9', { params: {} });
    expect(data.players).toEqual([]);
  });
});
