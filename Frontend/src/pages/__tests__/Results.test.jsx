import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Results from '../Results.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn() }));
vi.mock('../../services/eventService.js', () => ({ listEventsByTournament: vi.fn() }));
vi.mock('../../services/resultsService.js', () => ({ getEventResults: vi.fn() }));

import { listEventsByTournament } from '../../services/eventService.js';
import { getEventResults } from '../../services/resultsService.js';
import { listTournaments } from '../../services/tournamentService.js';

describe('Results Page', () => {
  beforeEach(() => vi.clearAllMocks());

  it('loads tournaments list', async () => {
    listTournaments.mockResolvedValueOnce([{ id: 1, name: 'Championship' }]);
    renderWithRouter(<Results />, { route: '/results', routes: [{ path: '/results', element: <Results /> }] });
    expect(await screen.findByText('Championship')).toBeInTheDocument();
  });

  it('shows event results after selecting tournament', async () => {
    listTournaments.mockResolvedValueOnce([{ id: 2, name: 'Summer Cup' }]);
    listEventsByTournament.mockResolvedValueOnce([{ id: 5, name: '100m' }]);
    getEventResults.mockResolvedValueOnce([
      { playerId: 10, playerName: 'P1', universityName: 'Uni A', timeMs: 12000, points: 10 },
      { playerId: 11, playerName: 'P2', universityName: 'Uni B', timeMs: 0, points: 0 }
    ]);
    renderWithRouter(<Results />, { route: '/results', routes: [{ path: '/results', element: <Results /> }] });
    const select = await screen.findByLabelText(/Select Tournament/i);
    fireEvent.change(select, { target: { value: '2' } });
    expect(await screen.findByText('100m')).toBeInTheDocument();
    expect(await screen.findByText('P1')).toBeInTheDocument();
    expect(await screen.findByText('P2')).toBeInTheDocument();
  });
});
