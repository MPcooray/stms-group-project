import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../playerEventsService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() } }));

describe('playerEventsService', () => {
  it('listEventsByPlayer validates playerId', async () => {
    await expect(svc.listEventsByPlayer()).rejects.toThrow(/playerId/);
  });
  it('addEventToPlayer validates name', async () => {
    await expect(svc.addEventToPlayer(1,'')).rejects.toThrow(/Event name/);
  });
  it('workflow add/update/delete', async () => {
    api.post.mockResolvedValueOnce({ data: { id: 10, event: 'A'} });
    const added = await svc.addEventToPlayer(5,' Swim ');
    expect(api.post).toHaveBeenCalledWith('/api/players/5/events', { event: 'Swim' });
    api.put.mockResolvedValueOnce({ data: { id: 10, event: 'B'} });
    await svc.updatePlayerEvent(10,'B');
    api.delete.mockResolvedValueOnce({ data: true });
    await svc.deletePlayerEvent(10);
    expect(added.event).toBe('A');
  });
});
