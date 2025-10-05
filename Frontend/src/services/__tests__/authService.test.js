import { describe, expect, it, vi } from 'vitest';
import api from '../apiClient.js';
import * as auth from '../authService.js';

vi.mock('../apiClient.js', () => ({ default: { post: vi.fn(), get: vi.fn() } }));

describe('authService', () => {
  it('login extracts token (direct)', async () => {
    api.post.mockResolvedValueOnce({ data: { token: 't1' } });
    const res = await auth.login('a','b');
    expect(res.token).toBe('t1');
  });
  it('login extracts nested token', async () => {
    api.post.mockResolvedValueOnce({ data: { data: { token: 'nested' } } });
    const res = await auth.login('a','b');
    expect(res.token).toBe('nested');
  });
  it('login throws when no token', async () => {
    api.post.mockResolvedValueOnce({ data: {} });
    await expect(auth.login('a','b')).rejects.toThrow(/No token/);
  });
  it('getProfile returns data', async () => {
    api.get.mockResolvedValueOnce({ data: { email: 'e@x.com' } });
    const p = await auth.getProfile();
    expect(p.email).toBe('e@x.com');
  });
  it('getProfile fallback when error', async () => {
    api.get.mockRejectedValueOnce(new Error('fail'));
    const p = await auth.getProfile();
    expect(p.role).toBe('Admin');
  });
});
