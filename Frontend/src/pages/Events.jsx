import { useEffect, useState } from "react"
import { Link, useParams } from "react-router-dom"
import DashboardLayout from "../components/DashboardLayout.jsx"
import { createEvent, deleteEvent, listEventsByTournament, updateEvent } from "../services/tournamentEventsService.js"

const empty = { name: "" }

export default function Events() {
  const { tournamentId } = useParams()
  const [items, setItems] = useState([])
  const [form, setForm] = useState(empty)
  const [editingId, setEditingId] = useState(null)
  const [status, setStatus] = useState("")

  const load = async () => {
    setStatus("")
    try {
  const defs = await listEventsByTournament(tournamentId)
  setItems(Array.isArray(defs) ? defs : [])
    } catch (err) {
      const msg = err?.response?.data?.error || err?.message || "Failed to load events"
      setStatus(msg)
      // eslint-disable-next-line no-console
      console.error("Events load failed:", err)
    }
  }

  useEffect(() => {
    load()
  }, [tournamentId])

  const onSubmit = async (e) => {
    e.preventDefault()
    if (!form.name.trim()) return setStatus("Event name is required")
    try {
      if (editingId) {
        await updateEvent(editingId, { name: form.name })
        setStatus("Event updated successfully ✔")
      } else {
        await createEvent(tournamentId, { name: form.name })
        setStatus("Event added successfully ✔")
      }
      setForm(empty)
      setEditingId(null)
      await load()
    } catch (error) {
      const msg = error?.response?.data?.error || error?.message || "Save failed"
      setStatus(msg)
      // eslint-disable-next-line no-console
      console.error("Event save failed:", error)
    }
  }

  const onEdit = (it) => {
    setForm({ name: it.name })
    setEditingId(it.id)
  }

  const onDelete = async (id) => {
    if (!confirm("Delete this event?")) return
    try {
      await deleteEvent(id)
      setStatus("Event deleted successfully ✔")
      await load()
    } catch (err) {
      const msg = err?.response?.data?.error || err?.message || "Delete failed"
      setStatus(msg)
      // eslint-disable-next-line no-console
      console.error("Event delete failed:", err)
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Events for Tournament ID: {tournamentId}</h2>
          {status && <div className={status.includes("✔") ? "success" : "error"}>{status}</div>}
          <div className="row">
            <div className="card" style={{ flex: 1, minWidth: 320 }}>
              <h3>{editingId ? "Edit Event" : "Add Event"}</h3>
              <form onSubmit={onSubmit}>
                <label>Name</label>
                <input
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="Enter event name (e.g., 100m Freestyle)"
                />
                <div className="space"></div>
                <button className="btn primary">{editingId ? "Update" : "Add"}</button>
                {editingId && (
                  <button
                    type="button"
                    className="btn ghost"
                    style={{ marginLeft: 8 }}
                    onClick={() => {
                      setEditingId(null)
                      setForm(empty)
                    }}
                  >
                    Cancel
                  </button>
                )}
                <div className="space"></div>
                {status && <div className={status.includes("✔") ? "success" : "error"}>{status}</div>}
              </form>
            </div>
            <div className="card" style={{ flex: 2, minWidth: 480 }}>
              <h3>All Events</h3>
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((it) => (
                    <tr key={it.id}>
                      <td>{it.name}</td>
                      <td>
                        <Link to={`/events/${tournamentId}/${it.id}/timings`} className="btn ghost">
                          Timings & Rankings
                        </Link>{" "}
                        <button className="btn ghost" onClick={() => onEdit(it)}>
                          Edit
                        </button>{" "}
                        <button className="btn danger" onClick={() => onDelete(it.id)}>
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))}
                  {items.length === 0 && (
                    <tr>
                      <td colSpan="2" className="muted">
                        No events yet.
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