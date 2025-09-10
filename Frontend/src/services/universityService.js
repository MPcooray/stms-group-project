import api from "./apiClient.js"

export async function listUniversitiesByTournament(tournamentId) {
  if (!tournamentId) throw new Error("tournamentId is required")
  const res = await api.get(`/api/tournaments/${tournamentId}/universities`)
  return res.data
}

export async function getUniversityById(id) {
  if (!id) throw new Error("id is required")
  const res = await api.get(`/api/universities/${id}`)
  return res.data
}

export async function createUniversity(tournamentId, payload) {
  if (!tournamentId) throw new Error("tournamentId is required")
  if (!payload?.name) throw new Error("University name is required")
  const res = await api.post(`/api/tournaments/${tournamentId}/universities`, { name: payload.name })
  return res.data
}

export async function updateUniversity(id, payload) {
  if (!id) throw new Error("id is required")
  if (!payload?.name) throw new Error("University name is required")
  const res = await api.put(`/api/universities/${id}`, { name: payload.name })
  return res.data
}

export async function deleteUniversity(id) {
  if (!id) throw new Error("id is required")
  const res = await api.delete(`/api/universities/${id}`)
  return res.data
}
