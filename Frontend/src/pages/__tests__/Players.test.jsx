import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Players from '../Players.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/playerService.js', () => ({
  listPlayersByUniversity: vi.fn(),
  createPlayer: vi.fn(),
  updatePlayer: vi.fn(),
  deletePlayer: vi.fn(),
}));
vi.mock('../../services/universityService.js', () => ({ getUniversityById: vi.fn() }));

import { createPlayer, deletePlayer, listPlayersByUniversity, updatePlayer } from '../../services/playerService.js';
import { getUniversityById } from '../../services/universityService.js';

function renderPage(tournamentId='1', universityId='2') {
  return renderWithRouter(<Players />, { route: `/universities/${tournamentId}/${universityId}/players`, routes: [{ path: '/universities/:tournamentId/:universityId/players', element: <Players /> }] });
}

describe('Players Page', () => {
  beforeEach(() => vi.clearAllMocks());

  it('lists players', async () => {
    listPlayersByUniversity.mockResolvedValueOnce([{ id: 1, name: 'Player One', gender: 'Male', age: 20 }]);
    getUniversityById.mockResolvedValueOnce({ id: 2, name: 'Uni X' });
    renderPage();
    expect(await screen.findByText('Player One')).toBeInTheDocument();
  });

  it('creates player', async () => {
    listPlayersByUniversity
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([{ id: 5, name: 'New Player', gender: 'Female', age: 19 }]);
    getUniversityById.mockResolvedValue({ id: 2, name: 'Uni X' });
    createPlayer.mockResolvedValueOnce({ id: 5 });
    renderPage();
    await screen.findByText(/No players yet/i);
    const nameInput = screen.getAllByRole('textbox')[0];
    fireEvent.change(nameInput, { target: { value: 'New Player' } });
    fireEvent.click(screen.getByRole('button', { name: /Create/i }));
    expect(createPlayer).toHaveBeenCalled();
    expect(await screen.findByText('New Player')).toBeInTheDocument();
  });

  it('edits player', async () => {
    listPlayersByUniversity
      .mockResolvedValueOnce([{ id: 6, name: 'Old Name', gender: 'Male', age: 25 }])
      .mockResolvedValueOnce([{ id: 6, name: 'Updated Name', gender: 'Male', age: 25 }]);
    getUniversityById.mockResolvedValueOnce({ id: 2, name: 'Uni X' });
    updatePlayer.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Old Name');
    fireEvent.click(screen.getByRole('button', { name: /Edit/i }));
    const nameInput = screen.getByDisplayValue('Old Name');
    fireEvent.change(nameInput, { target: { value: 'Updated Name' } });
    fireEvent.click(screen.getByRole('button', { name: /Update/i }));
    expect(updatePlayer).toHaveBeenCalled();
    expect(await screen.findByText('Updated Name')).toBeInTheDocument();
  });

  it('deletes player', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    listPlayersByUniversity
      .mockResolvedValueOnce([{ id: 7, name: 'Delete P', gender: 'Female', age: 22 }])
      .mockResolvedValueOnce([]);
    getUniversityById.mockResolvedValueOnce({ id: 2, name: 'Uni X' });
    deletePlayer.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Delete P');
    fireEvent.click(screen.getByRole('button', { name: /Delete/i }));
    expect(deletePlayer).toHaveBeenCalled();
    expect(await screen.findByText(/No players yet/i)).toBeInTheDocument();
  });
});
