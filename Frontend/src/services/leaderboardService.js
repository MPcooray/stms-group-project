import api from "./apiClient.js"

export async function getLeaderboard(tournamentId) {
  const res = await api.get(`/api/leaderboard/${tournamentId}`)
  return res.data
}
