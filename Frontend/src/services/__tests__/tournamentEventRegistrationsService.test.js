import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../tournamentEventRegistrationsService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn(), post: vi.fn(), delete: vi.fn() } }));

describe('tournamentEventRegistrationsService', () => {
  it('validates required params', async () => {
    await expect(svc.listRegistrations()).rejects.toThrow(/required/);
    await expect(svc.registerPlayer()).rejects.toThrow(/required/);
    await expect(svc.unregisterPlayer()).rejects.toThrow(/required/);
  });
  it('listRegistrations endpoint', async () => {
    api.get.mockResolvedValueOnce({ data: [] });
    await svc.listRegistrations(1,2);
    expect(api.get).toHaveBeenCalledWith('/api/tournaments/1/events/2/registrations');
  });
  it('register/unregister endpoints', async () => {
    api.post.mockResolvedValueOnce({ data: { ok:true } });
    await svc.registerPlayer(1,2,3);
    expect(api.post).toHaveBeenCalledWith('/api/tournaments/1/events/2/registrations', { playerId:3 });
    api.delete.mockResolvedValueOnce({ data: true });
    await svc.unregisterPlayer(1,2,3);
    expect(api.delete).toHaveBeenCalledWith('/api/tournaments/1/events/2/registrations/3');
  });
});
