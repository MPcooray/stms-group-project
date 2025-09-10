import { useEffect, useState } from "react"
import { Link } from "react-router-dom"
import DashboardLayout from "../components/DashboardLayout.jsx"
import { listTournaments } from "../services/tournamentService.js"

export default function Dashboard() {
  const [tournaments, setTournaments] = useState([])
  const [status, setStatus] = useState("")
  const [now, setNow] = useState(new Date())

  useEffect(() => {
    const load = async () => {
      setStatus("")
      try {
        const data = await listTournaments()
        // sort ascending by start date if present
        setTournaments(
          (Array.isArray(data) ? data : []).sort((a, b) =>
            new Date(a.startDate || a.date || 0) - new Date(b.startDate || b.date || 0),
          ),
        )
      } catch {
        setStatus("Failed to load tournaments")
      }
    }
    load()
  }, [])

  // live clock for countdowns
  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 1000)
    return () => clearInterval(id)
  }, [])

  function endOfDay(dateLike) {
    if (!dateLike) return null
    const d = new Date(dateLike)
    d.setHours(23, 59, 59, 999)
    return d
  }

  function fmtDuration(ms) {
    const s = Math.max(0, Math.floor(ms / 1000))
    const d = Math.floor(s / 86400)
    const h = Math.floor((s % 86400) / 3600)
    const m = Math.floor((s % 3600) / 60)
    if (d > 0) return `${d}d ${h}h ${m}m`
    if (h > 0) return `${h}h ${m}m`
    return `${m}m`
  }

  function countdownFor(t) {
    const start = new Date(t.startDate || t.date)
    const end = t.endDate ? endOfDay(t.endDate) : null
    if (Number.isNaN(start.getTime())) return ""
    if (now < start) return `Starts in ${fmtDuration(start - now)}`
    if (end && now <= end) return `Ends in ${fmtDuration(end - now)}`
    if (end && now > end) return `Ended ${fmtDuration(now - end)} ago`
    return `Started ${fmtDuration(now - start)} ago`
  }

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Dashboard Overview</h2>
          <p className="muted">Welcome to the Swimming Tournament Management System.</p>
          <div className="space"></div>
          <div className="row">
            <div className="card" style={{ flex: 1 }}>
              <h3>Select Tournament</h3>
              {status && <div className="error">{status}</div>}
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Location</th>
                    <th>Start</th>
                    <th>End</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
          {tournaments.map((t) => (
                    <tr key={t.id}>
                      <td>{t.name}</td>
                      <td>{t.location}</td>
            <td>{(t.startDate || t.date || "").toString().substring(0, 10)}</td>
            <td>{(t.endDate || "").toString().substring(0, 10)}</td>
                      <td className="muted">{countdownFor(t)}</td>
                      <td>
                        <Link to={`/universities/${t.id}`} className="btn ghost">
                          Universities
                        </Link>{" "}
                        <Link to={`/events/${t.id}`} className="btn ghost">
                          Events
                        </Link>{" "}
                        <Link to={`/players/${t.id}`} className="btn ghost">
                          All Players
                        </Link>
                      </td>
                    </tr>
                  ))}
                  {tournaments.length === 0 && (
                    <tr>
                      <td colSpan="6" className="muted">
                        No tournaments yet.
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