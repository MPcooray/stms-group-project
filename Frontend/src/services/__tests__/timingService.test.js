import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../timingService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn(), post: vi.fn() } }));

describe('timingService', () => {
  it('getTiming calls endpoint', async () => {
    api.get.mockResolvedValueOnce({ data: { timeMs: 1000 } });
    const r = await svc.getTiming(1,2);
    expect(api.get).toHaveBeenCalledWith('/api/timings/1/2');
    expect(r.timeMs).toBe(1000);
  });
  it('saveTiming posts payload', async () => {
    api.post.mockResolvedValueOnce({ data: { ok: true } });
    const r = await svc.saveTiming(1,2,3000);
    expect(api.post).toHaveBeenCalledWith('/api/timings', { playerId:1, eventId:2, timeMs:3000 });
    expect(r.ok).toBe(true);
  });
});
