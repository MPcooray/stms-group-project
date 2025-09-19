import api from "./apiClient.js"

// Get timing for a player and eventId
export async function getTiming(playerId, eventId) {
  const response = await api.get(`/api/timings/${playerId}/${eventId}`)
  return response.data
}

// Save timing for a player and eventId
export async function saveTiming(playerId, eventId, timeMs) {
  const response = await api.post("/api/timings", { playerId, eventId, timeMs })
  return response.data
}
