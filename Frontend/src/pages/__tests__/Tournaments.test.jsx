import { fireEvent, screen, waitFor } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Tournaments from '../Tournaments.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/tournamentService.js', () => ({
  listTournaments: vi.fn(),
  createTournament: vi.fn(),
  updateTournament: vi.fn(),
  deleteTournament: vi.fn()
}));

import { createTournament, deleteTournament, listTournaments, updateTournament } from '../../services/tournamentService.js';

describe('Tournaments Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  function renderPage() {
    return renderWithRouter(<Tournaments />, { route: '/tournaments', routes: [{ path: '/tournaments', element: <Tournaments /> }] });
  }

  it('lists tournaments', async () => {
    // Provide UI-shaped object with location so component renders location cell
    listTournaments.mockResolvedValueOnce([
      { id: 1, name: 'Test Cup', location: 'Pool A', startDate: '2025-09-10', endDate: '2025-09-11' }
    ]);
    renderPage();
    expect(await screen.findByText('Test Cup')).toBeInTheDocument();
  });

  it('shows empty state when no tournaments', async () => {
    listTournaments.mockResolvedValueOnce([]);
    renderPage();
    expect(await screen.findByText(/No tournaments yet/i)).toBeInTheDocument();
  });

  it('creates tournament (happy path)', async () => {
    listTournaments
      .mockResolvedValueOnce([]) // first load
      .mockResolvedValueOnce([ // reload after create
        { id: 2, name: 'Created Cup', location: 'X', startDate: '2025-10-01', endDate: '2025-10-02' }
      ]);
    createTournament.mockResolvedValueOnce({ id: 2 });
    renderPage();
    await screen.findByText(/No tournaments yet/i);
    const nameInput = document.getElementById('tournamentName');
    const locInput = document.getElementById('tournamentVenue');
    const startInput = document.getElementById('tournamentDate');
    fireEvent.change(nameInput, { target: { value: 'Created Cup' } });
    fireEvent.change(locInput, { target: { value: 'X' } });
    fireEvent.change(startInput, { target: { value: '2025-10-01' } });
    fireEvent.click(screen.getByRole('button', { name: /Create/i }));

    expect(createTournament).toHaveBeenCalled();
    expect(await screen.findByText('Created Cup')).toBeInTheDocument();
  });

  it('validates missing name', async () => {
    listTournaments.mockResolvedValueOnce([]);
    renderPage();
    await screen.findByText(/No tournaments yet/i);
    fireEvent.click(screen.getByRole('button', { name: /Create/i }));
    expect(await screen.findByText(/Tournament name is required/i)).toBeInTheDocument();
  });

  it('edits tournament', async () => {
    listTournaments
      .mockResolvedValueOnce([
        { id: 3, name: 'Initial', location: 'Loc', startDate: '2025-10-05' }
      ])
      .mockResolvedValueOnce([
        { id: 3, name: 'Updated', location: 'Loc', startDate: '2025-10-05' }
      ]);
    updateTournament.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Initial');
    fireEvent.click(screen.getByRole('button', { name: /Edit/i }));
    const nameInput = document.getElementById('tournamentName');
    fireEvent.change(nameInput, { target: { value: 'Updated' } });
    fireEvent.click(screen.getByRole('button', { name: /Update/i }));
    expect(updateTournament).toHaveBeenCalled();
    expect(await screen.findByText('Updated')).toBeInTheDocument();
  });

  it('deletes tournament', async () => {
    // mock confirm to auto-accept
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    listTournaments
      .mockResolvedValueOnce([
        { id: 4, name: 'DeleteMe', location: 'Loc', startDate: '2025-10-05' }
      ])
      .mockResolvedValueOnce([]);
    deleteTournament.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('DeleteMe');
    fireEvent.click(screen.getByRole('button', { name: /Delete/i }));
    await waitFor(() => expect(deleteTournament).toHaveBeenCalled());
    expect(await screen.findByText(/No tournaments yet/i)).toBeInTheDocument();
  });
});
