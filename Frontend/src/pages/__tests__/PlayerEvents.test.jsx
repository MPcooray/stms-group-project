import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import PlayerEvents from '../PlayerEvents.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/playerEventsService.js', () => ({
  listEventsByPlayer: vi.fn(),
  addEventToPlayer: vi.fn(),
  updatePlayerEvent: vi.fn(),
  deletePlayerEvent: vi.fn(),
}));

import { addEventToPlayer, deletePlayerEvent, listEventsByPlayer, updatePlayerEvent } from '../../services/playerEventsService.js';

function renderPage(playerId='10') {
  return renderWithRouter(<PlayerEvents />, { route: `/player-events/${playerId}`, routes: [{ path: '/player-events/:playerId', element: <PlayerEvents /> }] });
}

describe('PlayerEvents Page', () => {
  beforeEach(() => vi.clearAllMocks());

  it('lists player events', async () => {
    listEventsByPlayer.mockResolvedValueOnce([{ id: 1, event: '100m' }]);
    renderPage();
    expect(await screen.findByText('100m')).toBeInTheDocument();
  });

  it('adds event', async () => {
    listEventsByPlayer
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([{ id: 2, event: '200m' }]);
    addEventToPlayer.mockResolvedValueOnce({ id: 2 });
    renderPage();
    await screen.findByText(/No events yet/i);
    const input = screen.getByPlaceholderText(/e.g., 100m/i);
    fireEvent.change(input, { target: { value: '200m' } });
    fireEvent.click(screen.getByRole('button', { name: /Add/i }));
    expect(addEventToPlayer).toHaveBeenCalled();
    expect(await screen.findByText('200m')).toBeInTheDocument();
  });

  it('edits event', async () => {
    listEventsByPlayer
      .mockResolvedValueOnce([{ id: 3, event: 'Old' }])
      .mockResolvedValueOnce([{ id: 3, event: 'New' }]);
    updatePlayerEvent.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Old');
    fireEvent.click(screen.getByRole('button', { name: /Edit/i }));
    const input = screen.getByDisplayValue('Old');
    fireEvent.change(input, { target: { value: 'New' } });
    fireEvent.click(screen.getByRole('button', { name: /Update/i }));
    expect(updatePlayerEvent).toHaveBeenCalled();
    expect(await screen.findByText('New')).toBeInTheDocument();
  });

  it('deletes event', async () => {
    listEventsByPlayer
      .mockResolvedValueOnce([{ id: 4, event: 'Del' }])
      .mockResolvedValueOnce([]);
    deletePlayerEvent.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Del');
    fireEvent.click(screen.getByRole('button', { name: /Delete/i }));
    expect(deletePlayerEvent).toHaveBeenCalled();
    expect(await screen.findByText(/No events yet/i)).toBeInTheDocument();
  });
});
