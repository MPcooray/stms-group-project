import { useEffect, useState } from "react"
import { useParams, Link } from "react-router-dom"
import DashboardLayout from "../components/DashboardLayout.jsx"
import { listPlayersByUniversity, createPlayer, updatePlayer, deletePlayer } from "../services/playerService.js"
import { getUniversityById } from "../services/universityService.js"

const empty = { name: "", gender: "Male", age: "" }

export default function Players() {
  const { tournamentId, universityId } = useParams()
  const [items, setItems] = useState([])
  const [form, setForm] = useState(empty)
  const [editingId, setEditingId] = useState(null)
  const [status, setStatus] = useState("")
  const [universityName, setUniversityName] = useState("")

  const load = async () => {
    setStatus("")
    try {
      const data = await listPlayersByUniversity(universityId)
      setItems(Array.isArray(data) ? data : [])
    } catch {
      setStatus("Failed to load players")
    }
  }

  useEffect(() => {
    const run = async () => {
      await load()
      try {
        const uni = await getUniversityById(universityId)
        setUniversityName(uni?.name || "")
      } catch {
        setUniversityName("")
      }
    }
    run()
  }, [tournamentId, universityId])

  const onSubmit = async (e) => {
    e.preventDefault()

    if (!form.name.trim()) {
      setStatus("Player name is required")
      return
    }
  // University is fixed by route; no field needed
    if (form.age && (form.age < 1 || form.age > 120)) {
      setStatus("Age must be between 1 and 120")
      return
    }

    try {
  const payload = { name: form.name.trim(), age: form.age ? Number.parseInt(form.age) : null, gender: form.gender }
      if (editingId) {
        await updatePlayer(editingId, payload)
        setStatus("Player updated successfully ✔")
      } else {
        await createPlayer(universityId, payload)
        setStatus("Player created successfully ✔")
      }
      setForm(empty)
      setEditingId(null)
      await load()
    } catch (error) {
      setStatus(error.message || "Save failed")
    }
  }

  const onEdit = (it) => {
    setForm({
      name: it.name || "",
  gender: it.gender || "Male",
      age: it.age || "",
    })
    setEditingId(it.id || it.playerId)
  }

  const onDelete = async (id) => {
    if (!confirm("Delete this player?")) return
    try {
      await deletePlayer(id)
      setStatus("Player deleted successfully ✔")
      await load()
    } catch {
      setStatus("Delete failed")
    }
  }

  // University name comes from API now

  return (
    <DashboardLayout>
      <div className="container">
        <h2>Players for {universityName} (Tournament ID: {tournamentId})</h2>
        <div className="row">
          <div className="card" style={{ flex: 1, minWidth: 320 }}>
            <h3>{editingId ? "Edit Player" : "Create Player"}</h3>
            <form onSubmit={onSubmit}>
              <label>Name</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              <div className="space"></div>
              <div className="muted">University: <strong>{universityName || `#${universityId}`}</strong></div>
              <div className="space"></div>
              <label>Gender</label>
              <select value={form.gender} onChange={(e) => setForm({ ...form, gender: e.target.value })}>
                <option>Male</option>
                <option>Female</option>
              </select>
              <div className="space"></div>
              <label>Age (optional)</label>
              <input
                type="number"
                min="1"
                max="100"
                value={form.age}
                onChange={(e) => setForm({ ...form, age: e.target.value })}
              />
              <div className="space"></div>
              <button className="btn primary">{editingId ? "Update" : "Create"}</button>
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
            <h3>All Players</h3>
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>University</th>
                  <th>Gender</th>
                  <th>Age</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((it) => (
                  <tr key={it.id || it.playerId || it.name}>
                    <td>{it.name}</td>
                    <td>{universityName}</td>
                    <td>{it.gender ?? "-"}</td>
                    <td>{it.age ?? "-"}</td>
                    <td>
                      <Link className="btn ghost" to={`/players/${it.id}/events`}>
                        Events
                      </Link>{" "}
                      <button className="btn ghost" onClick={() => onEdit(it)}>
                        Edit
                      </button>{" "}
                      <button className="btn danger" onClick={() => onDelete(it.id || it.playerId)}>
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
                {items.length === 0 && (
                  <tr>
                    <td colSpan="5" className="muted">
                      No players yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </DashboardLayout>
  )
}