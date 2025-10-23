import { describe, expect, it } from 'vitest';
import api from '../apiClient.js';

describe('apiClient interceptor', () => {
  it('attaches Authorization header when token present', () => {
    localStorage.setItem('token','abc123');
    const cfg = api.interceptors.request.handlers[0].fulfilled({ headers: {} });
    expect(cfg.headers.Authorization).toBe('Bearer abc123');
  });

  it('skips Authorization header when no token', () => {
    localStorage.removeItem('token');
    const cfg = api.interceptors.request.handlers[0].fulfilled({ headers: {} });
    expect(cfg.headers.Authorization).toBeUndefined();
  });
});
