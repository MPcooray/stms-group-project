import { fireEvent, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import PlayersByUniversity from '../PlayersByUniversity.jsx'

// Mock DashboardLayout to simplify DOM
vi.mock('../../components/DashboardLayout.jsx', () => ({ default: ({ children }) => <div data-testid="layout">{children}</div> }))

// Helper to render with a given tournamentId route param
function renderWithTournament(tournamentId = '1') {
  return render(
    <MemoryRouter initialEntries={[`/tournaments/${tournamentId}/players-by-university`]}> 
      <Routes>
        <Route path='/tournaments/:tournamentId/players-by-university' element={<PlayersByUniversity />} />
      </Routes>
    </MemoryRouter>
  )
}

// The component seeds dummy players keyed by tournamentId; verify listing + CRUD simulation

describe('PlayersByUniversity Page', () => {
  beforeEach(() => {
    vi.restoreAllMocks()
  })

  it('lists players for tournament 1', async () => {
    renderWithTournament('1')
    // Should show heading with tournament id
    expect(await screen.findByText(/Tournament ID: 1/)).toBeInTheDocument()
    // Table rows (3 seeded players)
    const rows = await screen.findAllByRole('row')
    // first row is header, then 3 data rows
    expect(rows.length).toBeGreaterThanOrEqual(4)
    expect(screen.getByText('John Doe')).toBeInTheDocument()
    expect(screen.getByText('Jane Smith')).toBeInTheDocument()
    expect(screen.getByText('Alice Brown')).toBeInTheDocument()
  })

  it('shows no players for unknown tournament', async () => {
    renderWithTournament('999')
    expect(await screen.findByText(/Tournament ID: 999/)).toBeInTheDocument()
    expect(await screen.findByText(/No players yet/i)).toBeInTheDocument()
  })

  it('validates form fields', async () => {
    renderWithTournament('1')
    const submitBtn = await screen.findByRole('button', { name: /Create/i })
    fireEvent.click(submitBtn)
    expect(screen.getByText(/Player name is required/)).toBeInTheDocument()
  })

  it('creates a new player when form valid', async () => {
    renderWithTournament('1')
    const textboxes = await screen.findAllByRole('textbox')
    fireEvent.change(textboxes[0], { target: { value: 'New Player' } }) // Name
    fireEvent.change(textboxes[1], { target: { value: 'University A' } }) // University
    fireEvent.change(screen.getByRole('spinbutton'), { target: { value: '25' } }) // Age
    fireEvent.click(screen.getByRole('button', { name: /Create/i }))
    expect(await screen.findByText(/Player created successfully/)).toBeInTheDocument()
    expect(screen.getByText('New Player')).toBeInTheDocument()
  })

  it('edits an existing player', async () => {
    renderWithTournament('1')
    const editBtn = await screen.findAllByRole('button', { name: /Edit/i })
    fireEvent.click(editBtn[0])
    const textboxes = screen.getAllByRole('textbox')
    fireEvent.change(textboxes[0], { target: { value: 'John X' } })
    fireEvent.click(screen.getByRole('button', { name: /Update/i }))
    expect(await screen.findByText(/Player updated successfully/)).toBeInTheDocument()
    expect(screen.getByText('John X')).toBeInTheDocument()
  })

  it('deletes a player (confirm mocked yes)', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true)
    renderWithTournament('1')
    expect(await screen.findByText('John Doe')).toBeInTheDocument()
    const deleteBtns = await screen.findAllByRole('button', { name: /Delete/i })
    fireEvent.click(deleteBtns[0])
    expect(await screen.findByText(/Player deleted successfully/)).toBeInTheDocument()
    // John Doe should be gone now
    expect(screen.queryByText('John Doe')).not.toBeInTheDocument()
  })
})
