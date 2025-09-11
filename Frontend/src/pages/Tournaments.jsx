"use client"

import { useEffect, useState } from "react"
import { listTournaments, createTournament, updateTournament, deleteTournament } from "../services/tournamentService.js"
import DashboardLayout from "../components/DashboardLayout.jsx"

const empty = { name: "", location: "", startDate: "", endDate: "" }

export default function Tournaments() {
  const [items, setItems] = useState([])
  const [form, setForm] = useState(empty)
  const [editingId, setEditingId] = useState(null)
  const [status, setStatus] = useState("")

  const load = async () => {
    setStatus("")
    try {
      const data = await listTournaments()
      setItems(Array.isArray(data) ? data : [])
    } catch {
      setStatus("Failed to load tournaments")
    }
  }
  
  useEffect(() => {
    load()
  }, [])

  const onSubmit = async (e) => {
    e.preventDefault()

    if (!form.name.trim()) {
      setStatus("Tournament name is required")
      return
    }
    if (!form.location.trim()) {
      setStatus("Tournament location is required")
      return
    }
  if (form.startDate && form.endDate && form.startDate > form.endDate) {
      setStatus("End date must be after start date")
      return
    }

    try {
      if (editingId) {
        await updateTournament(editingId, form)
        setStatus("Tournament updated successfully ✔")
      } else {
        await createTournament(form)
        setStatus("Tournament created successfully ✔")
      }
      await new Promise(resolve => setTimeout(resolve, 800)) // slight delay to show success message
      setForm(empty)
      setEditingId(null)
      load()
    } catch (error) {
      setStatus(error.message || "Save failed")
    }
  }

  const onEdit = (it) => {
    setForm({
      name: it.name || "",
      location: it.location || "",
      startDate: it.startDate?.substring(0, 10) || "",
      endDate: it.endDate?.substring(0, 10) || "",
    })
    setEditingId(it.id || it.tournamentId)
  }

  const onDelete = async (id) => {
    if (!confirm("Delete this tournament?")) return
    try {
      await deleteTournament(id)
      setStatus("Deleted ✔")
      load()
    } catch (error) {
      if (error.response?.status === 404) {
        setStatus("Tournament not found")
      } else {
        setStatus("Delete failed")
      }
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <h2>Tournaments</h2>
        <div className="row">
          <div className="card" style={{ flex: 1, minWidth: 320 }}>
            <h3>{editingId ? "Edit Tournament" : "Create Tournament"}</h3>
            <form onSubmit={onSubmit}>
              <label>Name</label>
              <input id="tournamentName" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              <div className="space"></div>
              <label>Location</label>
              <input id="tournamentName" value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
              <div className="space"></div>
              <label>Start Date</label>
              <input
                id="tournamentDate"
                type="date"
                value={form.startDate}
                onChange={(e) => setForm({ ...form, startDate: e.target.value })}
              />
              <div className="space"></div>
              <label>End Date</label>
              <input type="date" value={form.endDate} onChange={(e) => setForm({ ...form, endDate: e.target.value })} />
              <div className="space"></div>
              <button id="saveTournamentButton" className="btn primary">{editingId ? "Update" : "Create"}</button>
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
            <h3>All Tournaments</h3>
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Location</th>
                  <th>Start</th>
                  <th>End</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((it) => (
                  <tr key={it.id || it.tournamentId || it.name}>
                    <td>{it.name}</td>
                    <td>{it.location}</td>
                    <td>{(it.startDate || "").toString().substring(0, 10)}</td>
                    <td>{(it.endDate || "").toString().substring(0, 10)}</td>
                    <td>
                      <button className="btn ghost" onClick={() => onEdit(it)}>
                        Edit
                      </button>{" "}
                      <button className="btn danger" onClick={() => onDelete(it.id || it.tournamentId)}>
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
                {items.length === 0 && (
                  <tr>
                    <td colSpan="5" className="muted">
                      No tournaments yet.
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
