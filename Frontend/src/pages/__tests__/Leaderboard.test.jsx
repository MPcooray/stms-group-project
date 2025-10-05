import { screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Leaderboard from '../Leaderboard.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/leaderboardService.js', () => ({ getLeaderboard: vi.fn() }));
vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn() }));

import { getLeaderboard } from '../../services/leaderboardService.js';
import { listTournaments } from '../../services/tournamentService.js';

describe('Leaderboard Page', () => {
  beforeEach(() => vi.clearAllMocks());

  it('lists tournaments when no tournamentId param', async () => {
    listTournaments.mockResolvedValueOnce([{ id: 1, name: 'Cup', location: 'Loc' }]);
    renderWithRouter(<Leaderboard />, { route: '/leaderboard', routes: [{ path: '/leaderboard', element: <Leaderboard /> }] });
    expect(await screen.findByText('Cup')).toBeInTheDocument();
  });

  it('shows leaderboard when tournamentId present', async () => {
    getLeaderboard.mockResolvedValueOnce({ players: [{ id: 1, name: 'A', university: 'U', totalPoints: 10 }], universities: [{ id: 9, name: 'U', totalPoints: 10 }] });
    renderWithRouter(<Leaderboard />, { route: '/leaderboard/1', routes: [{ path: '/leaderboard/:tournamentId', element: <Leaderboard /> }] });
    expect(await screen.findByText(/Player Leaderboard/i)).toBeInTheDocument();
    expect(await screen.findByText('A')).toBeInTheDocument();
  });
});
