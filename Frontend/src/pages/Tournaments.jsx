import { useEffect, useState } from 'react'
import { listTournaments, createTournament, updateTournament, deleteTournament } from '../services/tournamentService.js'

const empty = { name: '', location: '', startDate: '', endDate: '' }

export default function Tournaments(){
  const [items, setItems] = useState([])
  const [form, setForm] = useState(empty)
  const [editingId, setEditingId] = useState(null)
  const [status, setStatus] = useState('')

  const load = async () => {
    setStatus('')
    try {
      const data = await listTournaments()
      setItems(Array.isArray(data) ? data : [])
    } catch { setStatus('Failed to load tournaments') }
  }
  useEffect(()=>{ load() }, [])

  const onSubmit = async (e) => {
    e.preventDefault()
    try {
      if (editingId){ await updateTournament(editingId, form); setStatus('Updated ✔') }
      else { await createTournament(form); setStatus('Created ✔') }
      setForm(empty); setEditingId(null); load()
    } catch { setStatus('Save failed') }
  }

  const onEdit = (it) => {
    setForm({
      name: it.name || '',
      location: it.location || '',
      startDate: it.startDate?.substring(0,10) || '',
      endDate: it.endDate?.substring(0,10) || ''
    })
    setEditingId(it.id || it.tournamentId)
  }

  const onDelete = async (id) => {
    if (!confirm('Delete this tournament?')) return
    try { await deleteTournament(id); setStatus('Deleted ✔'); load() } catch { setStatus('Delete failed') }
  }

  return (
    <div className="container">
      <h2>Tournaments</h2>
      <div className="row">
        <div className="card" style={{ flex: 1, minWidth: 320 }}>
          <h3>{editingId ? 'Edit Tournament' : 'Create Tournament'}</h3>
          <form onSubmit={onSubmit}>
            <label>Name</label>
            <input value={form.name} onChange={e=>setForm({ ...form, name: e.target.value })} />
            <div className="space"></div>
            <label>Location</label>
            <input value={form.location} onChange={e=>setForm({ ...form, location: e.target.value })} />
            <div className="space"></div>
            <label>Start Date</label>
            <input type="date" value={form.startDate} onChange={e=>setForm({ ...form, startDate: e.target.value })} />
            <div className="space"></div>
            <label>End Date</label>
            <input type="date" value={form.endDate} onChange={e=>setForm({ ...form, endDate: e.target.value })} />
            <div className="space"></div>
            <button className="btn primary">{editingId ? 'Update' : 'Create'}</button>
            {editingId && <button type="button" className="btn ghost" style={{marginLeft:8}} onClick={()=>{setEditingId(null);setForm(empty)}}>Cancel</button>}
            <div className="space"></div>
            {status && <div className={status.includes('✔') ? 'success' : 'error'}>{status}</div>}
          </form>
        </div>
        <div className="card" style={{ flex: 2, minWidth: 480 }}>
          <h3>All Tournaments</h3>
          <table>
            <thead><tr><th>Name</th><th>Location</th><th>Start</th><th>End</th><th>Actions</th></tr></thead>
            <tbody>
              {items.map(it => (
                <tr key={it.id || it.tournamentId || it.name}>
                  <td>{it.name}</td>
                  <td>{it.location}</td>
                  <td>{(it.startDate || '').toString().substring(0,10)}</td>
                  <td>{(it.endDate || '').toString().substring(0,10)}</td>
                  <td>
                    <button className="btn ghost" onClick={()=>onEdit(it)}>Edit</button>{' '}
                    <button className="btn danger" onClick={()=>onDelete(it.id || it.tournamentId)}>Delete</button>
                  </td>
                </tr>
              ))}
              {items.length === 0 && <tr><td colSpan="5" className="muted">No tournaments yet.</td></tr>}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  )
}
