import { fireEvent, screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import Universities from '../Universities.jsx';

vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div>{children}</div> }));
vi.mock('../../services/universityService.js', () => ({
  listUniversitiesByTournament: vi.fn(),
  createUniversity: vi.fn(),
  updateUniversity: vi.fn(),
  deleteUniversity: vi.fn(),
}));

import { createUniversity, deleteUniversity, listUniversitiesByTournament, updateUniversity } from '../../services/universityService.js';

function renderPage(tournamentId='1') {
  return renderWithRouter(<Universities />, { route: `/universities/${tournamentId}`, routes: [{ path: '/universities/:tournamentId', element: <Universities /> }] });
}

describe('Universities Page', () => {
  beforeEach(() => vi.clearAllMocks());

  it('lists universities', async () => {
    listUniversitiesByTournament.mockResolvedValueOnce([{ id: 1, name: 'Uni A' }]);
    renderPage();
    expect(await screen.findByText('Uni A')).toBeInTheDocument();
  });

  it('shows empty state', async () => {
    listUniversitiesByTournament.mockResolvedValueOnce([]);
    renderPage();
    expect(await screen.findByText(/No universities registered/i)).toBeInTheDocument();
  });

  it('creates university', async () => {
    listUniversitiesByTournament
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([{ id: 2, name: 'Uni B' }]);
    createUniversity.mockResolvedValueOnce({ id: 2 });
    renderPage();
    await screen.findByText(/No universities registered/i);
    const input = screen.getByPlaceholderText(/enter university name/i);
    fireEvent.change(input, { target: { value: 'Uni B' } });
    fireEvent.click(screen.getByRole('button', { name: /Add/i }));
    expect(createUniversity).toHaveBeenCalled();
    expect(await screen.findByText('Uni B')).toBeInTheDocument();
  });

  it('edits university', async () => {
    listUniversitiesByTournament
      .mockResolvedValueOnce([{ id: 3, name: 'Old Uni' }])
      .mockResolvedValueOnce([{ id: 3, name: 'New Uni' }]);
    updateUniversity.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Old Uni');
    fireEvent.click(screen.getByRole('button', { name: /Edit/i }));
    const input = screen.getByDisplayValue('Old Uni');
    fireEvent.change(input, { target: { value: 'New Uni' } });
    fireEvent.click(screen.getByRole('button', { name: /Update/i }));
    expect(updateUniversity).toHaveBeenCalled();
    expect(await screen.findByText('New Uni')).toBeInTheDocument();
  });

  it('deletes university', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);
    listUniversitiesByTournament
      .mockResolvedValueOnce([{ id: 4, name: 'Del Uni' }])
      .mockResolvedValueOnce([]);
    deleteUniversity.mockResolvedValueOnce({});
    renderPage();
    await screen.findByText('Del Uni');
    fireEvent.click(screen.getByRole('button', { name: /Delete/i }));
    expect(deleteUniversity).toHaveBeenCalled();
    expect(await screen.findByText(/No universities registered/i)).toBeInTheDocument();
  });
});
