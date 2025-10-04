import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import PublicTournamentResults from '../PublicTournamentResults.jsx';

vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn() }));
vi.mock('../../services/eventService.js', () => ({ listEventsByTournament: vi.fn() }));
vi.mock('../../services/resultsService.js', () => ({ getEventResults: vi.fn() }));

import { listEventsByTournament } from '../../services/eventService.js';
import { getEventResults } from '../../services/resultsService.js';
import { listTournaments } from '../../services/tournamentService.js';

describe('PublicTournamentResults', () => {
  beforeEach(() => vi.clearAllMocks());

  const routes = [
    { path: '/public/tournaments/:tournamentId/results', element: <PublicTournamentResults /> }
  ];

  it('shows not found when tournament missing', async () => {
    listTournaments.mockResolvedValueOnce([]);
    renderWithRouter(<PublicTournamentResults />, { route: '/public/tournaments/55/results', routes });
    expect(await screen.findByText(/Tournament Not Found/i)).toBeInTheDocument();
  });

  it('renders events and results then switches to overview tab', async () => {
    listTournaments.mockResolvedValueOnce([{ id: 9, name: 'Results Cup', venue: 'Pool X', date: '2025-09-10' }]);
    listEventsByTournament.mockResolvedValueOnce([
      { id: 100, name: '100m Freestyle' }
    ]);
    getEventResults.mockResolvedValueOnce([
      { playerId: 1, eventId: 100, playerName: 'Alice', universityName: 'Uni A', timeMs: 58100, points: 10 }
    ]);

    renderWithRouter(<PublicTournamentResults />, { route: '/public/tournaments/9/results', routes });

    expect(await screen.findByText('100m Freestyle')).toBeInTheDocument();
  expect(screen.getByText('Alice')).toBeInTheDocument();
  expect(screen.getByText('10')).toBeInTheDocument();

    const overviewTab = screen.getByRole('button', { name: /Overview/i });
    fireEvent.click(overviewTab);
  expect(await screen.findByText(/Tournament Information/i)).toBeInTheDocument();
  // Assert main heading specifically to avoid multiple matches containing the text
  expect(screen.getByRole('heading', { level: 1, name: /Results Cup/ })).toBeInTheDocument();
  });
});
