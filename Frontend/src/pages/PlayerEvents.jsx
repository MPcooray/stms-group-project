import DashboardLayout from "../components/DashboardLayout.jsx"
import { useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import { listEventsByPlayer, addEventToPlayer, updatePlayerEvent, deletePlayerEvent } from "../services/playerEventsService.js"

export default function PlayerEvents() {
  const { playerId } = useParams()
  const [items, setItems] = useState([])
  const [name, setName] = useState("")
  const [editingId, setEditingId] = useState(null)
  const [status, setStatus] = useState("")

  const load = async () => {
    setStatus("")
    try {
      const data = await listEventsByPlayer(playerId)
      setItems(Array.isArray(data) ? data : [])
    } catch {
      setStatus("Failed to load events")
    }
  }

  useEffect(() => {
    load()
  }, [playerId])

  const onSubmit = async (e) => {
    e.preventDefault()
    if (!name.trim()) return setStatus("Event name is required")
    try {
      if (editingId) {
        await updatePlayerEvent(editingId, name)
        setStatus("Event updated ✔")
      } else {
        await addEventToPlayer(playerId, name)
        setStatus("Event added ✔")
      }
      setName("")
      setEditingId(null)
      await load()
    } catch (e) {
      setStatus(e.message || "Save failed")
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Player Events (Player #{playerId})</h2>
          <div className="row">
            <div className="card" style={{ flex: 1, minWidth: 320 }}>
              <h3>{editingId ? "Edit Event" : "Add Event"}</h3>
              <form onSubmit={onSubmit}>
                <label>Event Name</label>
                <input value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g., 100m Freestyle" />
                <div className="space"></div>
                <button className="btn primary">{editingId ? "Update" : "Add"}</button>
                {editingId && (
                  <button type="button" className="btn ghost" style={{ marginLeft: 8 }} onClick={() => { setEditingId(null); setName("") }}>
                    Cancel
                  </button>
                )}
                <div className="space"></div>
                {status && <div className={status.includes("✔") ? "success" : "error"}>{status}</div>}
              </form>
            </div>
            <div className="card" style={{ flex: 2, minWidth: 480 }}>
              <h3>Registered Events</h3>
              <table>
                <thead>
                  <tr>
                    <th>Event</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((it) => (
                    <tr key={it.id}>
                      <td>{it.event}</td>
                      <td>
                        <button className="btn ghost" onClick={() => { setEditingId(it.id); setName(it.event) }}>Edit</button>{" "}
                        <button className="btn danger" onClick={async () => { await deletePlayerEvent(it.id); await load() }}>Delete</button>
                      </td>
                    </tr>
                  ))}
                  {items.length === 0 && (
                    <tr>
                      <td colSpan="2" className="muted">No events yet.</td>
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
