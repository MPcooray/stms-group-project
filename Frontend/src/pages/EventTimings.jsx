import DashboardLayout from "../components/DashboardLayout.jsx"
import { useState, useEffect } from "react"
import { useParams } from "react-router-dom"

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
        // Dummy available players based on tournamentId
        const dummyAvailable = {
          1: [
            { id: 1, name: "John Doe", university: "University A" },
            { id: 2, name: "Jane Smith", university: "University A" },
            { id: 4, name: "Alice Brown", university: "University B" },
          ],
          2: [
            { id: 3, name: "Mike Lee", university: "University C" },
            { id: 5, name: "Bob Johnson", university: "University D" },
          ],
        }

        // Dummy registered players with timings based on tournamentId-eventId
        const dummyRegistered = {
          "1-1": [
            { id: 1, name: "John Doe", university: "University A", timing: 50.5 },
          ],
          "1-2": [
            { id: 2, name: "Jane Smith", university: "University A", timing: null },
          ],
          "2-3": [
            { id: 3, name: "Mike Lee", university: "University C", timing: 25.3 },
          ],
          "2-4": [],
        }

        const avail = dummyAvailable[tournamentId] || []
        const regKey = `${tournamentId}-${eventId}`
        const reg = dummyRegistered[regKey] || []

        // Filter available to exclude registered
        const filteredAvail = avail.filter(
          (p) => !reg.some((r) => r.id === p.id)
        )

        setAvailablePlayers(filteredAvail)
        setRegisteredPlayers(reg)
      } catch {
        setStatus("Failed to load data")
      }
    }
    load()
  }, [tournamentId, eventId])

  const onAddPlayer = (e) => {
    e.preventDefault()
    if (!selectedPlayerId) {
      setStatus("Select a player to register")
      return
    }
    const playerToAdd = availablePlayers.find((p) => p.id === parseInt(selectedPlayerId))
    if (playerToAdd) {
      setRegisteredPlayers([...registeredPlayers, { ...playerToAdd, timing: null }])
      setAvailablePlayers(availablePlayers.filter((p) => p.id !== playerToAdd.id))
      setSelectedPlayerId("")
      setStatus("Player registered successfully ✔")
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