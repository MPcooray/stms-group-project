"use client"

import { useState } from "react"
import DashboardLayout from "../components/DashboardLayout.jsx"

const emptyEvent = { event1: "", event2: "", event3: "" }

export default function EventTimings({ playerId }) {
  const [form, setForm] = useState({ event1: "120.5", event2: "130.2", event3: "115.8" }) // Dummy initial values
  const [status, setStatus] = useState("")

  const onSubmit = async (e) => {
    e.preventDefault()
    if (!form.event1 || !form.event2 || !form.event3) {
      setStatus("All event timings are required")
      return
    }
    try {
      setStatus("Timings saved successfully ✔")
      setForm(emptyEvent)
    } catch {
      setStatus("Failed to save timings")
    }
  }

  return (
    <DashboardLayout>
      <div className="container">
        <h2>Enter Timings for Player</h2>
        <div className="card" style={{ flex: 1, minWidth: 320 }}>
          <form onSubmit={onSubmit}>
            <label>Event 1 Timing (seconds)</label>
            <input
              type="number"
              value={form.event1}
              onChange={(e) => setForm({ ...form, event1: e.target.value })}
              min="0"
              step="0.1"
            />
            <div className="space"></div>
            <label>Event 2 Timing (seconds)</label>
            <input
              type="number"
              value={form.event2}
              onChange={(e) => setForm({ ...form, event2: e.target.value })}
              min="0"
              step="0.1"
            />
            <div className="space"></div>
            <label>Event 3 Timing (seconds)</label>
            <input
              type="number"
              value={form.event3}
              onChange={(e) => setForm({ ...form, event3: e.target.value })}
              min="0"
              step="0.1"
            />
            <div className="space"></div>
            <button className="btn primary">Save Timings</button>
            <div className="space"></div>
            {status && <div className={status.includes("✔") ? "success" : "error"}>{status}</div>}
          </form>
        </div>
      </div>
    </DashboardLayout>
  )
}