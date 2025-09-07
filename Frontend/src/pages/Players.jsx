"use client"

import { useEffect, useState } from "react"
import { listPlayers, createPlayer, updatePlayer, deletePlayer } from "../services/playerService.js"
import DashboardLayout from "../components/DashboardLayout.jsx"

const empty = { name: "", university: "", gender: "Male", age: "" }

export default function Players() {
  const [items, setItems] = useState([])
  const [form, setForm] = useState(empty)
  const [editingId, setEditingId] = useState(null)
  const [status, setStatus] = useState("")

  const load = async () => {
    setStatus("")
    try {
      const data = await listPlayers()
      setItems(Array.isArray(data) ? data : [])
    } catch {
      setStatus("Failed to load players")
    }
  }
  useEffect(() => {
    load()
  }, [])

  const onSubmit = async (e) => {
    e.preventDefault()

    if (!form.name.trim()) {
      setStatus("Player name is required")
      return
    }
    if (!form.university.trim()) {
      setStatus("University is required")
      return
    }
    if (form.age && (form.age < 1 || form.age > 100)) {
      setStatus("Age must be between 1 and 100")
      return
    }

    try {
      const payload = {
        ...form,
        age: form.age ? Number.parseInt(form.age) : null,
      }

      if (editingId) {
        await updatePlayer(editingId, payload)
        setStatus("Player updated successfully ✔")
      } else {
        await createPlayer(payload)
        setStatus("Player created successfully ✔")
      }
      setForm(empty)
      setEditingId(null)
      load()
    } catch (error) {
      setStatus(error.message || "Save failed")
    }
  }

  const onEdit = (it) => {
    setForm({ name: it.name || "", university: it.university || "", gender: it.gender || "Male", age: it.age || "" })
    setEditingId(it.id || it.playerId)
  }

  const onDelete = async (id) => {
    if (!confirm("Delete this player?")) return
    try {
      await deletePlayer(id)
      setStatus("Player deleted successfully ✔")
      load()
    } catch {
      setStatus("Delete failed")
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <h2>Players</h2>
        <div className="row">
          <div className="card" style={{ flex: 1, minWidth: 320 }}>
            <h3>{editingId ? "Edit Player" : "Create Player"}</h3>
            <form onSubmit={onSubmit}>
              <label>Name</label>
              <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              <div className="space"></div>
              <label>University</label>
              <input value={form.university} onChange={(e) => setForm({ ...form, university: e.target.value })} />
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
                  <tr key={it.id || it.playerId || it.name + it.university}>
                    <td>{it.name}</td>
                    <td>{it.university}</td>
                    <td>{it.gender}</td>
                    <td>{it.age || "-"}</td>
                    <td>
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
