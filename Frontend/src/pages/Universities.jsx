import DashboardLayout from "../components/DashboardLayout.jsx"
import { useState, useEffect } from "react"
import { useParams, Link } from "react-router-dom"
import { listUniversitiesByTournament, createUniversity, updateUniversity, deleteUniversity } from "../services/universityService.js"

export default function Universities() {
  const { tournamentId } = useParams()
  const [universities, setUniversities] = useState([])
  const [form, setForm] = useState({ name: "" })
  const [editingId, setEditingId] = useState(null)
  const [status, setStatus] = useState("")

  useEffect(() => {
    const load = async () => {
      setStatus("")
      try {
        const data = await listUniversitiesByTournament(tournamentId)
        setUniversities(Array.isArray(data) ? data : [])
      } catch {
        setStatus("Failed to load universities")
      }
    }
    load()
  }, [tournamentId])

  const onSubmit = async (e) => {
    e.preventDefault()
    if (!form.name.trim()) {
      setStatus("University name is required")
      return
    }
    try {
      if (editingId) {
        await updateUniversity(editingId, { name: form.name })
        setStatus("University updated successfully ✔")
      } else {
        await createUniversity(tournamentId, { name: form.name })
        setStatus("University added successfully ✔")
      }
      setForm({ name: "" })
      setEditingId(null)
      const data = await listUniversitiesByTournament(tournamentId)
      setUniversities(Array.isArray(data) ? data : [])
    } catch (error) {
      setStatus(error.message || "Save failed")
    }
  }

  const onEdit = (uni) => {
    setForm({ name: uni.name })
    setEditingId(uni.id)
  }

  const onDelete = async (id) => {
    if (!confirm("Delete this university?")) return
    try {
      await deleteUniversity(id)
      setStatus("University deleted successfully ✔")
      const data = await listUniversitiesByTournament(tournamentId)
      setUniversities(Array.isArray(data) ? data : [])
    } catch {
      setStatus("Delete failed")
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <div className="card">
          <h2>Universities for Tournament ID: {tournamentId}</h2>
          <div className="row">
            <div className="card" style={{ flex: 1, minWidth: 320 }}>
              <h3>{editingId ? "Edit University" : "Add University"}</h3>
              <form onSubmit={onSubmit}>
                <label>Name</label>
                <input
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="Enter university name"
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
                      setForm({ name: "" })
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
              <h3>Registered Universities</h3>
              <table>
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {universities.map((uni) => (
                    <tr key={uni.id}>
                      <td>{uni.name}</td>
                      <td>
                        <Link to={`/universities/${tournamentId}/${uni.id}/players`} className="btn ghost">
                          View Players
                        </Link>{" "}
                        <button className="btn ghost" onClick={() => onEdit(uni)}>
                          Edit
                        </button>{" "}
                        <button className="btn danger" onClick={() => onDelete(uni.id)}>
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))}
                  {universities.length === 0 && (
                    <tr>
                      <td colSpan="2" className="muted">
                        No universities registered yet.
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