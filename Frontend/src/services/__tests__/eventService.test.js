import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as svc from '../eventService.js';
vi.mock('../apiClient.js', () => ({ default: { get: vi.fn() } }));

describe('eventService', () => {
  it('listEventsByTournament hits endpoint', async () => {
    api.get.mockResolvedValueOnce({ data: [{ id:1,name:'Ev'}] });
    const res = await svc.listEventsByTournament(5);
    expect(api.get).toHaveBeenCalledWith('/api/tournaments/5/events');
    expect(res[0].name).toBe('Ev');
  });
});
