import api from "./apiClient.js"

// List all events for a tournament
export async function listEventsByTournament(tournamentId) {
  const response = await api.get(`/api/tournaments/${tournamentId}/events`)
  return response.data
}
