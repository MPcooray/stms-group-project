import { screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { listTournaments } from '../../services/tournamentService.js';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import PublicHome from '../PublicHome.jsx';

vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn() }));

describe('PublicHome', () => {
  beforeEach(() => vi.clearAllMocks());

  it('shows loading then tournaments count and cards', async () => {
    listTournaments.mockResolvedValueOnce([
      { id: 1, name: 'Spring Meet', venue: 'Pool A', date: '2025-09-12' },
      { id: 2, name: 'Autumn Gala', venue: 'Pool B', date: '2025-11-01' }
    ]);
    renderWithRouter(<PublicHome />);
    expect(screen.getByText(/Loading tournaments/i)).toBeInTheDocument();
    expect(await screen.findByText('Spring Meet')).toBeInTheDocument();
    expect(screen.getByText('Autumn Gala')).toBeInTheDocument();
    expect(screen.getByText('2')).toBeInTheDocument();
  });

  it('shows empty state when no tournaments', async () => {
    listTournaments.mockResolvedValueOnce([]);
    renderWithRouter(<PublicHome />);
    expect(await screen.findByText(/No tournaments available/i)).toBeInTheDocument();
  });
});
