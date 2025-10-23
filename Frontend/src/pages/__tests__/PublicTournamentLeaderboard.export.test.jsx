import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';

// Mock dependencies before importing the component to avoid hoisting/init issues
vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn().mockResolvedValue([{ id: 5, name: 'Mock Cup', location: 'Arena', startDate: '2025-09-12' }]) }));
vi.mock('../../services/leaderboardService.js', () => ({ getLeaderboard: vi.fn().mockResolvedValue({ players: [ { id: 1, name: 'Alice', university: 'Uni A', totalPoints: 18, rank: 1 } ], universities: [ { id: 10, name: 'Uni A', totalPoints: 18, rank: 1 } ] }) }));

// Mock jsPDF and autoTable early. Create mocks inside the factory and export them so
// they don't rely on outer-scope variables (avoids hoisting/init ReferenceError).
vi.mock('jspdf', () => {
  const saveMockInner = vi.fn();
  return {
    default: vi.fn().mockImplementation(() => ({ setFontSize: vi.fn(), text: vi.fn(), save: saveMockInner })),
    __esModule: true,
    __saveMock: saveMockInner
  };
});
vi.mock('jspdf-autotable', () => {
  const autoTableMockInner = vi.fn();
  return {
    default: autoTableMockInner,
    __esModule: true,
    __autoTableMock: autoTableMockInner
  };
});

import PublicTournamentLeaderboard from '../PublicTournamentLeaderboard.jsx';
import { renderWithRouter } from '../../tests/testUtils.jsx';

describe('PublicTournamentLeaderboard PDF export', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('builds PDF with player table when Export to PDF clicked (players view)', async () => {
    const routes = [{ path: '/public/tournaments/:tournamentId/leaderboard', element: <PublicTournamentLeaderboard /> }];
    renderWithRouter(<PublicTournamentLeaderboard />, { route: '/public/tournaments/5/leaderboard', routes });

    expect(await screen.findByText(/Mock Cup - Leaderboard/)).toBeInTheDocument();

    const exportBtn = screen.getByRole('button', { name: /Export to PDF/i });
    fireEvent.click(exportBtn);

    // jsPDF constructor should have been called and autoTable invoked
  // Import the mocks exported from the mocked modules and assert they were used
  const { __autoTableMock: autoTableMock } = await import('jspdf-autotable');
  const { __saveMock: saveMock } = await import('jspdf');
  expect(autoTableMock).toHaveBeenCalled();
  expect(saveMock).toHaveBeenCalled();
  });

  it('builds PDF with university table when switched and Export clicked', async () => {
    const routes = [{ path: '/public/tournaments/:tournamentId/leaderboard', element: <PublicTournamentLeaderboard /> }];
    renderWithRouter(<PublicTournamentLeaderboard />, { route: '/public/tournaments/5/leaderboard', routes });

    expect(await screen.findByText(/Mock Cup - Leaderboard/)).toBeInTheDocument();

    const uniBtn = screen.getByRole('button', { name: /University Rankings/i });
    fireEvent.click(uniBtn);

    const exportBtn = screen.getByRole('button', { name: /Export to PDF/i });
    fireEvent.click(exportBtn);
    const { __autoTableMock: autoTableMock } = await import('jspdf-autotable');
    const { __saveMock: saveMock } = await import('jspdf');
    expect(autoTableMock).toHaveBeenCalled();
    expect(saveMock).toHaveBeenCalled();
  });
});
