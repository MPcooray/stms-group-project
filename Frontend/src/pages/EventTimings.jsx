import { useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import DashboardLayout from "../components/DashboardLayout.jsx"
import { listPlayersByTournament } from "../services/playerService.js"
import { listRegistrations, registerPlayer, unregisterPlayer } from "../services/tournamentEventRegistrationsService.js"

export default function EventTimings() {
  const { tournamentId, eventId } = useParams()
  const [registeredPlayers, setRegisteredPlayers] = useState([])
  const [availablePlayers, setAvailablePlayers] = useState([])
  const [selectedPlayerId, setSelectedPlayerId] = useState("")
  const [status, setStatus] = useState("")

  useEffect(() => {
    const load = async () => {
      setStatus("")
      try {
        const [allPlayers, regs] = await Promise.all([
          listPlayersByTournament(tournamentId),
          listRegistrations(tournamentId, eventId),
        ])

        const regIds = new Set((regs || []).map((r) => r.playerId))
        const registered = (regs || []).map((r) => ({
          id: r.playerId,
          name: r.playerName,
          university: r.universityName,
          timing: null,
        }))
        const available = (allPlayers || [])
          .filter((p) => !regIds.has(p.id))
          .map((p) => ({ id: p.id, name: p.name, university: p.universityName }))

        setRegisteredPlayers(registered)
        setAvailablePlayers(available)
      } catch (err) {
        console.error(err)
        setStatus("Failed to load data")
      }
    }
    load()
  }, [tournamentId, eventId])

  const onAddPlayer = async (e) => {
    e.preventDefault()
    if (!selectedPlayerId) return setStatus("Select a player to register")
    try {
      await registerPlayer(tournamentId, eventId, parseInt(selectedPlayerId))
      const playerToAdd = availablePlayers.find((p) => p.id === parseInt(selectedPlayerId))
      if (playerToAdd) {
        setRegisteredPlayers([...registeredPlayers, { ...playerToAdd, timing: null }])
        setAvailablePlayers(availablePlayers.filter((p) => p.id !== playerToAdd.id))
      }
      setSelectedPlayerId("")
      setStatus("Player registered successfully ✔")
    } catch (err) {
      const msg = err?.response?.data?.error || err?.message || "Registration failed"
      setStatus(msg)
    }
  }

  const updateTiming = (id, newTiming) => {
    const timingValue = newTiming ? parseFloat(newTiming) : null
    setRegisteredPlayers(
      registeredPlayers.map((p) =>
        p.id === id ? { ...p, timing: timingValue } : p
      )
    )
  }

  // Sorted players: by timing ascending, nulls at end
  const sortedPlayers = [...registeredPlayers].sort((a, b) => {
    const timeA = a.timing ?? Infinity
    const timeB = b.timing ?? Infinity
    return timeA - timeB
  })

  let currentRank = 1

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Timings & Rankings for Event ID: {eventId} (Tournament ID: {tournamentId})</h2>
          <div className="row">
            <div className="card" style={{ flex: 1, minWidth: 320 }}>
              <h3>Register Player for Event</h3>
              <form onSubmit={onAddPlayer}>
                <label>Select Player</label>
                <select
                  value={selectedPlayerId}
                  onChange={(e) => setSelectedPlayerId(e.target.value)}
                >
                  <option value="">-- Select Player --</option>
                  {availablePlayers.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.name} ({p.university})
                    </option>
                  ))}
                </select>
                <div className="space"></div>
                <button className="btn primary">Register</button>
                <div className="space"></div>
                {status && <div className={status.includes("✔") ? "success" : "error"}>{status}</div>}
              </form>
            </div>
            <div className="card" style={{ flex: 2, minWidth: 480 }}>
              <h3>Registered Players & Timings</h3>
              <table>
                <thead>
                  <tr>
                    <th>Rank</th>
                    <th>Name</th>
                    <th>University</th>
                    <th>Timing (seconds)</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {sortedPlayers.map((p) => {
                    const rank = p.timing !== null ? currentRank++ : "-"
                    return (
                      <tr key={p.id}>
                        <td>{rank}</td>
                        <td>{p.name}</td>
                        <td>{p.university}</td>
                        <td>
                          <input
                            type="number"
                            step="0.01"
                            min="0"
                            value={p.timing ?? ""}
                            onChange={(e) => updateTiming(p.id, e.target.value)}
                            placeholder="Enter time"
                          />
                        </td>
                        <td>
                          <button className="btn danger" onClick={async () => {
                            try {
                              await unregisterPlayer(tournamentId, eventId, p.id)
                              setRegisteredPlayers(registeredPlayers.filter((x) => x.id !== p.id))
                              setAvailablePlayers([...availablePlayers, { id: p.id, name: p.name, university: p.university }])
                              setStatus("Player unregistered ✔")
                            } catch (err) {
                              const msg = err?.response?.data?.error || err?.message || "Unregister failed"
                              setStatus(msg)
                            }
                          }}>
                            Remove
                          </button>
                        </td>
                      </tr>
                    )
                  })}
                  {sortedPlayers.length === 0 && (
                    <tr>
                      <td colSpan="4" className="muted">
                        No players registered yet.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </DashboardLayout>
  )
}