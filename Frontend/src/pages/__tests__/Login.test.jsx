import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Login from '../Login.jsx';

// Mock AuthContext hook
vi.mock('../../context/AuthContext.jsx', () => ({
  useAuth: () => ({ login: vi.fn().mockResolvedValue(undefined) })
}));

// Mock validators to keep logic simple
vi.mock('../../utils/validators.js', () => ({
  email: (v) => (v && v.includes('@') ? '' : 'Invalid email'),
  required: (v) => (v ? '' : 'Required')
}));

describe('Login Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('navigates to dashboard after successful login (button shows loading first)', async () => {
    renderWithRouter(<Login />, { 
      route: '/login', 
      routes: [
        { path: '/login', element: <Login /> },
        { path: '/dashboard', element: <div>Dashboard Page</div> }
      ] 
    });
    const btn = screen.getByRole('button', { name: /sign in/i });
    expect(screen.getByDisplayValue('admin@stms.com')).toBeInTheDocument();
    fireEvent.click(btn);
    expect(btn).toBeDisabled(); // loading state
    // After mock resolves we should land on dashboard route
    expect(await screen.findByText(/dashboard page/i)).toBeInTheDocument();
  });

  it('shows validation error if email cleared', async () => {
    renderWithRouter(<Login />, { route: '/login', routes: [{ path: '/login', element: <Login /> }] });
    const emailInput = screen.getByPlaceholderText('admin@stms.com');
    fireEvent.change(emailInput, { target: { value: '' } });
    const form = emailInput.closest('form');
    fireEvent.submit(form);
    expect(await screen.findByText(/Invalid email|Required/i)).toBeInTheDocument();
  });
});
