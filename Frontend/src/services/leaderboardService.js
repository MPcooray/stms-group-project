import api from "./apiClient.js"

export async function getLeaderboard(tournamentId, gender) {
  const params = {}
  if (gender && gender !== 'All') params.gender = gender
  const res = await api.get(`/api/leaderboard/${tournamentId}`, { params })
  return res.data
}
