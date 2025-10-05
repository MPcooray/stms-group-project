import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthProvider, useAuth } from '../AuthContext.jsx';

vi.mock('../../services/authService.js', () => ({
  login: vi.fn(async () => ({ token: 'jwt123' })),
  getProfile: vi.fn(async () => ({ email: 'loaded@user.com', role: 'Admin' })),
  logout: vi.fn(() => true)
}));

import { getProfile, login as loginApi, logout as logoutApi } from '../../services/authService.js';

function Consumer() {
  const { user, token, isAuthenticated, login, logout } = useAuth();
  return (
    <div>
      <div data-testid="auth-state">{JSON.stringify({ user, token, isAuthenticated })}</div>
      <button onClick={() => login('admin@stms.com','pass')} data-testid="login-btn">Login</button>
      <button onClick={() => logout()} data-testid="logout-btn">Logout</button>
    </div>
  );
}

function renderWithAuth(route='/') {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <Routes>
        <Route path="/*" element={<AuthProvider><Consumer /></AuthProvider>} />
      </Routes>
    </MemoryRouter>
  );
}

describe('AuthContext', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.clearAllMocks();
  });

  it('logs in and sets token + user profile', async () => {
    renderWithAuth();
    fireEvent.click(screen.getByTestId('login-btn'));
    await waitFor(() => expect(loginApi).toHaveBeenCalled());
    await waitFor(() => expect(getProfile).toHaveBeenCalledTimes(2)); // once from login, once from effect
    const state = screen.getByTestId('auth-state').textContent;
    expect(state).toMatch(/jwt123/);
    expect(state).toMatch(/loaded@user.com/);
  });

  it('logs out clears token and user', async () => {
    renderWithAuth();
    // simulate login first
    fireEvent.click(screen.getByTestId('login-btn'));
    await waitFor(() => expect(loginApi).toHaveBeenCalled());
    fireEvent.click(screen.getByTestId('logout-btn'));
    await waitFor(() => expect(logoutApi).toHaveBeenCalled());
    const stateAfter = screen.getByTestId('auth-state').textContent;
    expect(stateAfter).toMatch(/null/);
    expect(stateAfter).not.toMatch(/jwt123/);
  });
});
