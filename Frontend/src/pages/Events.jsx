import DashboardLayout from "../components/DashboardLayout.jsx"
import { useState, useEffect } from "react"
import { useParams, Link } from "react-router-dom"

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
      // Simulate API call with dummy data based on tournamentId
      const dummyData = {
        1: [
          { id: 1, name: "100m Freestyle" },
          { id: 2, name: "200m Backstroke" },
        ],
        2: [
          { id: 3, name: "50m Butterfly" },
          { id: 4, name: "400m Medley" },
        ],
      }
      const data = dummyData[tournamentId] || []
      setItems(data)
    } catch {
      setStatus("Failed to load events")
    }
  }

  useEffect(() => {
    load()
  }, [tournamentId])

  const onSubmit = async (e) => {
    e.preventDefault()
    if (!form.name.trim()) {
      setStatus("Event name is required")
      return
    }
    try {
      if (editingId) {
        setItems(
          items.map((it) =>
            it.id === editingId ? { ...it, name: form.name } : it
          )
        )
        setStatus("Event updated successfully ✔")
      } else {
        const newItem = {
          id: items.length + 1,
          name: form.name,
        }
        setItems([...items, newItem])
        setStatus("Event added successfully ✔")
      }
      setForm(empty)
      setEditingId(null)
    } catch (error) {
      setStatus("Save failed")
    }
  }

  const onEdit = (it) => {
    setForm({ name: it.name })
    setEditingId(it.id)
  }

  const onDelete = (id) => {
    if (!confirm("Delete this event?")) return
    try {
      setItems(items.filter((it) => it.id !== id))
      setStatus("Event deleted successfully ✔")
    } catch {
      setStatus("Delete failed")
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Events for Tournament ID: {tournamentId}</h2>
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