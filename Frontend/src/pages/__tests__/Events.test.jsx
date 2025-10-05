import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Events from '../Events.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/tournamentEventsService.js', () => ({
  listEventsByTournament: vi.fn(),
  createEvent: vi.fn(),
  updateEvent: vi.fn(),
  deleteEvent: vi.fn(),
}));

import { createEvent, deleteEvent, listEventsByTournament, updateEvent } from '../../services/tournamentEventsService.js';

function renderPage(tournamentId='1') {
  return renderWithRouter(<Events />, { route: `/events/${tournamentId}`, routes: [{ path: '/events/:tournamentId', element: <Events /> }] });
}

describe('Events Page', () => {
  beforeEach(() => vi.clearAllMocks());

  it('lists events', async () => {
    listEventsByTournament.mockResolvedValueOnce([{ id: 1, name: '100m Freestyle' }]);
    renderPage();
    expect(await screen.findByText('100m Freestyle')).toBeInTheDocument();
  });

  it('shows empty state', async () => {
    listEventsByTournament.mockResolvedValueOnce([]);
    renderPage();
    expect(await screen.findByText(/No events yet/i)).toBeInTheDocument();
  });

  it('creates event', async () => {
    listEventsByTournament
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([{ id: 2, name: '200m Backstroke' }]);
    createEvent.mockResolvedValueOnce({ id: 2 });
    renderPage();
    await screen.findByText(/No events yet/i);
    const nameInput = screen.getByPlaceholderText(/enter event name/i);
    fireEvent.change(nameInput, { target: { value: '200m Backstroke' } });
    fireEvent.click(screen.getByRole('button', { name: /add/i }));
    expect(createEvent).toHaveBeenCalled();
    expect(await screen.findByText('200m Backstroke')).toBeInTheDocument();
  });

  it('edits event', async () => {
    listEventsByTournament
      .mockResolvedValueOnce([{ id: 3, name: 'Initial Event' }])
      .mockResolvedValueOnce([{ id: 3, name: 'Updated Event' }]);
    updateEvent.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Initial Event');
    fireEvent.click(screen.getByRole('button', { name: /edit/i }));
    const nameInput = screen.getByDisplayValue('Initial Event');
    fireEvent.change(nameInput, { target: { value: 'Updated Event' } });
    fireEvent.click(screen.getByRole('button', { name: /update/i }));
    expect(updateEvent).toHaveBeenCalled();
    expect(await screen.findByText('Updated Event')).toBeInTheDocument();
  });

  it('deletes event', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    listEventsByTournament
      .mockResolvedValueOnce([{ id: 4, name: 'Delete Me' }])
      .mockResolvedValueOnce([]);
    deleteEvent.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Delete Me');
    fireEvent.click(screen.getByRole('button', { name: /delete/i }));
    expect(deleteEvent).toHaveBeenCalled();
    expect(await screen.findByText(/No events yet/i)).toBeInTheDocument();
  });
});
