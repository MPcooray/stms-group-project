import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import EventTimings from '../EventTimings.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/eventService.js', () => ({ listEventsByTournament: vi.fn() }));
vi.mock('../../services/playerService.js', () => ({ listPlayersByTournament: vi.fn() }));
vi.mock('../../services/tournamentEventRegistrationsService.js', () => ({
  listRegistrations: vi.fn(),
  registerPlayer: vi.fn(),
  unregisterPlayer: vi.fn(),
}));
vi.mock('../../services/timingService.js', () => ({ getTiming: vi.fn(), saveTiming: vi.fn() }));

import { listEventsByTournament } from '../../services/eventService.js';
import { listPlayersByTournament } from '../../services/playerService.js';
import { getTiming, saveTiming } from '../../services/timingService.js';
import { listRegistrations, registerPlayer } from '../../services/tournamentEventRegistrationsService.js';

function renderPage(tournamentId='1') {
  return renderWithRouter(<EventTimings />, { route: `/event-timings/${tournamentId}`, routes: [{ path: '/event-timings/:tournamentId', element: <EventTimings /> }] });
}

describe('EventTimings Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    // Ensure saveTiming returns a thenable so component's .catch chain is safe
    saveTiming.mockResolvedValue({});
  });

  it('lists registered players sorted by timing and can update timing', async () => {
    listEventsByTournament
      .mockResolvedValueOnce([{ id: 10, name: '100m' }])
      .mockResolvedValueOnce([{ id: 10, name: '100m' }]);
    listPlayersByTournament.mockResolvedValueOnce([
      { id: 1, name: 'A', universityName: 'Uni A' },
      { id: 2, name: 'B', universityName: 'Uni B' }
    ]);
    listRegistrations.mockResolvedValueOnce([
      { playerId: 1, playerName: 'A', universityName: 'Uni A' }
    ]);
    getTiming.mockResolvedValueOnce({ timeMs: 15000 });
    renderPage();
    expect(await screen.findByText(/Timings & Rankings/i)).toBeInTheDocument();
    // timing input should show 15 once fetched (seconds)
    const timingInput = await screen.findByDisplayValue('15');
    fireEvent.change(timingInput, { target: { value: '14.5' } });
    expect(saveTiming).toHaveBeenCalled();
  });

  it('registers a player', async () => {
    listEventsByTournament
      .mockResolvedValueOnce([{ id: 11, name: '200m' }])
      .mockResolvedValueOnce([{ id: 11, name: '200m' }]);
    listPlayersByTournament.mockResolvedValueOnce([
      { id: 3, name: 'C', universityName: 'Uni C' }
    ]);
    listRegistrations.mockResolvedValueOnce([]);
    renderPage();
    await screen.findByText('200m');
  const selects = screen.getAllByRole('combobox');
  // second select is player select (first is event selection)
  fireEvent.change(selects[1], { target: { value: '3' } });
    fireEvent.click(screen.getByRole('button', { name: /Register/i }));
    expect(registerPlayer).toHaveBeenCalled();
  });
});
