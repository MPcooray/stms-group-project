import api from "./apiClient.js"

export async function listEventsByPlayer(playerId) {
  if (!playerId) throw new Error("playerId is required")
  const res = await api.get(`/api/players/${playerId}/events`)
  return res.data
}

export async function addEventToPlayer(playerId, name) {
  if (!playerId) throw new Error("playerId is required")
  if (!name?.trim()) throw new Error("Event name is required")
  const res = await api.post(`/api/players/${playerId}/events`, { event: name.trim() })
  return res.data
}

export async function updatePlayerEvent(id, name) {
  if (!id) throw new Error("id is required")
  if (!name?.trim()) throw new Error("Event name is required")
  const res = await api.put(`/api/player-events/${id}`, { event: name.trim() })
  return res.data
}

export async function deletePlayerEvent(id) {
  if (!id) throw new Error("id is required")
  const res = await api.delete(`/api/player-events/${id}`)
  return res.data
}
