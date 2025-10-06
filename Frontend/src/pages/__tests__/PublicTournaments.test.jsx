import { screen } from '@testing-library/react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { listTournaments } from '../../services/tournamentService.js';
import { renderWithRouter } from '../../tests/testUtils.jsx';
import PublicTournaments from '../PublicTournaments.jsx';

vi.mock('../../services/tournamentService.js', () => ({ listTournaments: vi.fn() }));

describe('PublicTournaments', () => {
  beforeEach(() => vi.clearAllMocks());

  it('renders tournaments list', async () => {
    listTournaments.mockResolvedValueOnce([
      { id: 10, name: 'Championship', venue: 'Central Pool', date: '2025-12-15' }
    ]);
    renderWithRouter(<PublicTournaments />);
    expect(await screen.findByText('Championship')).toBeInTheDocument();
    expect(screen.getByText(/Central Pool/)).toBeInTheDocument();
  });

  it('renders empty state when no tournaments', async () => {
    listTournaments.mockResolvedValueOnce([]);
    renderWithRouter(<PublicTournaments />);
    expect(await screen.findByText(/No Tournaments Available/i)).toBeInTheDocument();
  });
});
