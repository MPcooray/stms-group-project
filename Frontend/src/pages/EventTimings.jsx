import { useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import DashboardLayout from "../components/DashboardLayout.jsx"
import { listPlayersByTournament } from "../services/playerService.js"
import { listRegistrations, registerPlayer, unregisterPlayer } from "../services/tournamentEventRegistrationsService.js"
import { getTiming, saveTiming } from "../services/timingService.js"
import { listEventsByTournament } from "../services/eventService.js"

export default function EventTimings() {
  const { tournamentId } = useParams()
  const [eventList, setEventList] = useState([])
  const [selectedEventId, setSelectedEventId] = useState("")
  const [registeredPlayers, setRegisteredPlayers] = useState([])
  const [availablePlayers, setAvailablePlayers] = useState([])
  const [selectedPlayerId, setSelectedPlayerId] = useState("")
  const [status, setStatus] = useState("")

  useEffect(() => {
    const load = async () => {
      setStatus("")
      try {
        const events = await listEventsByTournament(tournamentId)
        setEventList(events)
        // Default to first event if none selected
        if (!selectedEventId && events.length > 0) setSelectedEventId(events[0].id)
        if (!selectedEventId) return
        const [allPlayers, regs] = await Promise.all([
          listPlayersByTournament(tournamentId),
          listRegistrations(tournamentId, selectedEventId),
        ])
        const regIds = new Set((regs || []).map((r) => r.playerId))
        // Fetch timings for each registered player
        const registered = await Promise.all((regs || []).map(async (r) => {
          let timing = null
          try {
            const timingData = await getTiming(r.playerId, selectedEventId)
            timing = timingData ? timingData.timeMs / 1000 : null // convert ms to seconds
          } catch (err) {
            timing = null
          }
          return {
            id: r.playerId,
            name: r.playerName,
            university: r.universityName,
            timing,
          }
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
  }, [tournamentId, selectedEventId])

  const onAddPlayer = async (e) => {
    e.preventDefault()
    if (!selectedPlayerId) return setStatus("Select a player to register")
    try {
      await registerPlayer(tournamentId, selectedEventId, parseInt(selectedPlayerId))
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
    // Save timing to backend (convert to ms)
    if (timingValue !== null && selectedEventId) {
      saveTiming(id, selectedEventId, Math.round(timingValue * 1000)).catch(() => {})
    }
  }

  // Sorted players: by timing ascending, nulls at end
  const sortedPlayers = [...registeredPlayers].sort((a, b) => {
    const timeA = a.timing ?? Infinity
    const timeB = b.timing ?? Infinity
    return timeA - timeB
  })

  // Points by rank: 1st=10, 2nd=7, 3rd=5, 4th=3, 5th=1, others=0
  // Points by rank: 1st=10, 2nd=8, 3rd=7, 4th=5, 5th=4, 6th=3, 7th=2, 8th=1, others=0
  function getPoints(rank) {
    switch (rank) {
      case 1: return 10;
      case 2: return 8;
      case 3: return 7;
      case 4: return 5;
      case 5: return 4;
      case 6: return 3;
      case 7: return 2;
      case 8: return 1;
      default: return 0;
    }
  }

  // Calculate ranks with tie handling
  const rankedPlayers = [];
  let rank = 1;
  for (let i = 0; i < sortedPlayers.length; i++) {
    if (sortedPlayers[i].timing === null) {
      rankedPlayers.push({ ...sortedPlayers[i], rank: '-' });
      continue;
    }
    // Count ties
    let tieCount = 1;
    while (
      i + tieCount < sortedPlayers.length &&
      sortedPlayers[i].timing === sortedPlayers[i + tieCount].timing
    ) {
      tieCount++;
    }
    for (let j = 0; j < tieCount; j++) {
      rankedPlayers.push({ ...sortedPlayers[i + j], rank });
    }
    i += tieCount - 1;
    rank += tieCount;
  }

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Timings & Rankings (Tournament ID: {tournamentId})</h2>
          <div className="row">
            <div className="card" style={{ flex: 1, minWidth: 320 }}>
              <h3>Select Event</h3>
              <select value={selectedEventId} onChange={e => setSelectedEventId(e.target.value)}>
                {eventList.map(ev => (
                  <option key={ev.id} value={ev.id}>{ev.name}</option>
                ))}
              </select>
              <div className="space"></div>
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
                    <th>Points</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {rankedPlayers.map((p) => {
                    const points = typeof p.rank === "number" ? getPoints(p.rank) : "-";
                    return (
                      <tr key={p.id}>
                        <td>{p.rank}</td>
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
                        <td>{points}</td>
                        <td>
                          <button className="btn danger" onClick={async () => {
                            try {
                              await unregisterPlayer(tournamentId, selectedEventId, p.id)
                              setRegisteredPlayers(registeredPlayers.filter((x) => x.id !== p.id))
                              setAvailablePlayers([...availablePlayers, { id: p.id, name: p.name, university: p.university }])
                              setStatus("Player unregistered ✔")
                            } catch (err) {
                              const msg = err?.response?.data?.error || err?.message || "Unregister failed";
                              setStatus(msg);
                            }
                          }}>
                            Remove
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                  {sortedPlayers.length === 0 && (
                    <tr>
                      <td colSpan="5" className="muted">
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