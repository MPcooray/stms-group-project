import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import Navbar from '../Navbar.jsx';

// From this test file (components/__tests__) we need to go up two levels to reach src/context
vi.mock('../../context/AuthContext.jsx', () => ({ useAuth: () => ({ user: { email: 'nav@test.com' }, logout: vi.fn() }) }));

describe('Navbar', () => {
  it('shows user email and triggers logout', () => {
    render(<Navbar />);
    expect(screen.getByText(/nav@test.com/)).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: /Logout/i }));
  });
});
