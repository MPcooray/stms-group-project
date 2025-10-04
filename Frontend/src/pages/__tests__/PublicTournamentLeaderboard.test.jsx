import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { getLeaderboard } from '../../services/leaderboardService.js';
import { listTournaments } from '../../services/tournamentService.js';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import PublicTournamentLeaderboard from '../PublicTournamentLeaderboard.jsx';

vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn() }));
vi.mock('../../services/leaderboardService.js', () => ({ getLeaderboard: vi.fn() }));

describe('PublicTournamentLeaderboard', () => {
  beforeEach(() => vi.clearAllMocks());

  const routes = [
    { path: '/public/tournaments/:tournamentId/leaderboard', element: <PublicTournamentLeaderboard /> }
  ];

  it('shows not found when tournament missing', async () => {
    listTournaments.mockResolvedValueOnce([]);
    renderWithRouter(<PublicTournamentLeaderboard />, { route: '/public/tournaments/5/leaderboard', routes });
    expect(await screen.findByText(/Tournament Not Found/i)).toBeInTheDocument();
  });

  it('renders player leaderboard and toggles to universities', async () => {
    listTournaments.mockResolvedValueOnce([{ id: 5, name: 'Mock Cup', venue: 'Arena', date: '2025-09-12' }]);
    getLeaderboard.mockResolvedValueOnce({
      players: [ { id: 1, name: 'Alice', university: 'Uni A', totalPoints: 18 } ],
      universities: [ { id: 10, name: 'Uni A', totalPoints: 18 } ]
    });

    renderWithRouter(<PublicTournamentLeaderboard />, { route: '/public/tournaments/5/leaderboard', routes });

    expect(await screen.findByText(/Mock Cup - Leaderboard/)).toBeInTheDocument();
    expect(screen.getByText('Alice')).toBeInTheDocument();

    const uniBtn = screen.getByRole('button', { name: /University Rankings/i });
    fireEvent.click(uniBtn);
    expect(await screen.findByText(/University Leaderboard/i)).toBeInTheDocument();
    expect(screen.getByText('Uni A')).toBeInTheDocument();
  });
});
