import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../tournamentEventsService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() } }));

describe('tournamentEventsService', () => {
  it('validation errors', async () => {
    await expect(svc.listEventsByTournament()).rejects.toThrow(/tournamentId/);
    await expect(svc.createEvent()).rejects.toThrow(/tournamentId/);
    await expect(svc.updateEvent()).rejects.toThrow(/id/);
    await expect(svc.deleteEvent()).rejects.toThrow(/id/);
  });
  it('listEventsByTournament success', async () => {
    api.get.mockResolvedValueOnce({ data: [] });
    await svc.listEventsByTournament(5);
    expect(api.get).toHaveBeenCalledWith('/api/tournaments/5/events');
  });
  it('create/update/delete flow', async () => {
    api.post.mockResolvedValueOnce({ data: { id:1 } });
    await svc.createEvent(5,{ name:' Swim ' });
    expect(api.post).toHaveBeenCalledWith('/api/tournaments/5/events', { name:'Swim' });
    api.put.mockResolvedValueOnce({ data: { id:1 } });
    await svc.updateEvent(1,{ name:'Updated' });
    api.delete.mockResolvedValueOnce({ data:true });
    await svc.deleteEvent(1);
  });
});
