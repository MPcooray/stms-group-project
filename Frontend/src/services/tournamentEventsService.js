import api from "./apiClient.js"

export async function listEventsByTournament(tournamentId) {
  if (!tournamentId) throw new Error("tournamentId is required")
  const res = await api.get(`/api/tournaments/${tournamentId}/events`)
  return res.data
}

export async function createEvent(tournamentId, payload) {
  if (!tournamentId) throw new Error("tournamentId is required")
  if (!payload?.name?.trim()) throw new Error("Event name is required")
  const res = await api.post(`/api/tournaments/${tournamentId}/events`, { name: payload.name.trim() })
  return res.data
}

export async function updateEvent(id, payload) {
  if (!id) throw new Error("id is required")
  if (!payload?.name?.trim()) throw new Error("Event name is required")
  const res = await api.put(`/api/tournament-events/${id}`, { name: payload.name.trim() })
  return res.data
}

export async function deleteEvent(id) {
  if (!id) throw new Error("id is required")
  const res = await api.delete(`/api/tournament-events/${id}`)
  return res.data
}
