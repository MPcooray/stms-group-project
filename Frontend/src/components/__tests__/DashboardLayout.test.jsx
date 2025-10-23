import { fireEvent, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import DashboardLayout from '../DashboardLayout.jsx';

vi.mock('../../context/AuthContext.jsx', () => ({
  useAuth: () => ({ user: { email: 'admin@test.com' }, logout: vi.fn() })
}));

describe('DashboardLayout', () => {
  it('renders menu and user info, triggers logout', () => {
    const { container } = render(
      <MemoryRouter initialEntries={['/dashboard']}>
        <DashboardLayout>
          <div>Inner Content</div>
        </DashboardLayout>
      </MemoryRouter>
    );
  // Brand text was changed to AquaChamps in DashboardLayout
  expect(screen.getByText(/AquaChamps/)).toBeInTheDocument();
    expect(screen.getByText(/Welcome, admin@test.com/)).toBeInTheDocument();
    expect(screen.getByText(/Dashboard/)).toBeInTheDocument();
    fireEvent.click(screen.getByRole('button', { name: /Logout/i }));
  });
});
