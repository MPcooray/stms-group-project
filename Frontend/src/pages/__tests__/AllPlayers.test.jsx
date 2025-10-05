import { screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { listPlayersByTournament } from '../../services/playerService.js';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import AllPlayers from '../AllPlayers.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/playerService.js', () => ({ listPlayersByTournament: vi.fn() }));

describe('AllPlayers Page', () => {
  beforeEach(() => vi.clearAllMocks());

  const routes = [{ path: '/players/:tournamentId', element: <AllPlayers /> }];

  it('renders players', async () => {
    listPlayersByTournament.mockResolvedValueOnce([
      { id: 1, name: 'Alice', universityName: 'Uni A', gender: 'F', age: 20 }
    ]);
    renderWithRouter(<AllPlayers />, { route: '/players/5', routes });
    expect(await screen.findByText('Alice')).toBeInTheDocument();
    expect(screen.getByText('Uni A')).toBeInTheDocument();
  });

  it('shows empty state', async () => {
    listPlayersByTournament.mockResolvedValueOnce([]);
    renderWithRouter(<AllPlayers />, { route: '/players/7', routes });
    expect(await screen.findByText(/No players yet/i)).toBeInTheDocument();
  });
});
