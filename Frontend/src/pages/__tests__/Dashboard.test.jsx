import { screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { listTournaments } from '../../services/tournamentService.js';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Dashboard from '../Dashboard.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn() }));

describe('Dashboard Page', () => {
  beforeEach(() => vi.clearAllMocks());

  it('renders tournaments table rows', async () => {
    listTournaments.mockResolvedValueOnce([
      { id: 1, name: 'Alpha', location: 'Pool A', startDate: '2025-09-10' },
      { id: 2, name: 'Beta', location: 'Pool B', startDate: '2025-09-12' }
    ]);
    renderWithRouter(<Dashboard />, { route: '/dashboard', routes: [{ path: '/dashboard', element: <Dashboard /> }] });
    expect(await screen.findByText('Alpha')).toBeInTheDocument();
    expect(screen.getByText('Beta')).toBeInTheDocument();
  });

  it('shows empty state when none', async () => {
    listTournaments.mockResolvedValueOnce([]);
    renderWithRouter(<Dashboard />, { route: '/dashboard', routes: [{ path: '/dashboard', element: <Dashboard /> }] });
    expect(await screen.findByText(/No tournaments yet/i)).toBeInTheDocument();
  });
});
