import axios from 'axios';
import { describe, expect, it, vi } from 'vitest';
import * as svc from '../resultsService.js';

vi.mock('axios', () => ({ default: { get: vi.fn() }, get: vi.fn() }));

describe('resultsService', () => {
  it('getEventResults returns array and filters non-array', async () => {
    axios.get.mockResolvedValueOnce({ data: [{ id:1 }] });
    const arr = await svc.getEventResults(1);
    expect(arr.length).toBe(1);
    axios.get.mockResolvedValueOnce({ data: { not: 'array' } });
    const empty = await svc.getEventResults(2);
    expect(empty).toEqual([]);
  });
});
