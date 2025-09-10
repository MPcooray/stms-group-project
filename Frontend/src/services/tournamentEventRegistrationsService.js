import api from "./apiClient.js"

export async function listRegistrations(tournamentId, eventId) {
  if (!tournamentId || !eventId) throw new Error("tournamentId and eventId are required")
  const res = await api.get(`/api/tournaments/${tournamentId}/events/${eventId}/registrations`)
  return res.data
}

export async function registerPlayer(tournamentId, eventId, playerId) {
  if (!tournamentId || !eventId || !playerId) throw new Error("tournamentId, eventId, playerId are required")
  const res = await api.post(`/api/tournaments/${tournamentId}/events/${eventId}/registrations`, { playerId })
  return res.data
}

export async function unregisterPlayer(tournamentId, eventId, playerId) {
  if (!tournamentId || !eventId || !playerId) throw new Error("tournamentId, eventId, playerId are required")
  const res = await api.delete(`/api/tournaments/${tournamentId}/events/${eventId}/registrations/${playerId}`)
  return res.data
}
