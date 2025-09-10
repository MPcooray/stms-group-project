import { useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import DashboardLayout from "../components/DashboardLayout.jsx"
import { listPlayersByTournament } from "../services/playerService.js"

export default function AllPlayers() {
  const { tournamentId } = useParams()
  const [items, setItems] = useState([])
  const [status, setStatus] = useState("")

  const load = async () => {
    setStatus("")
    try {
      const data = await listPlayersByTournament(tournamentId)
      setItems(Array.isArray(data) ? data : [])
    } catch {
      setStatus("Failed to load players")
    }
  }

  useEffect(() => {
    load()
  }, [tournamentId])

  return (
    <DashboardLayout>
      <div className="container">
        <h2>All Players (Tournament ID: {tournamentId})</h2>
        <div className="card" style={{ minWidth: 480 }}>
          <h3>Players</h3>
          {status && <div className="error">{status}</div>}
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>University</th>
                <th>Gender</th>
                <th>Age</th>
              </tr>
            </thead>
            <tbody>
              {items.map((it) => (
                <tr key={it.id}>
                  <td>{it.name}</td>
                  <td>{it.universityName}</td>
                  <td>{it.gender || "-"}</td>
                  <td>{it.age ?? "-"}</td>
                </tr>
              ))}
              {items.length === 0 && (
                <tr>
                  <td colSpan="4" className="muted">No players yet.</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </DashboardLayout>
  )
}