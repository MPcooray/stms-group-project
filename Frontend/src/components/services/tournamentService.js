import api from './apiClient.js'

export async function listTournaments(){ return (await api.get('/tournaments')).data }
export async function createTournament(payload){ return (await api.post('/tournaments', payload)).data }
export async function updateTournament(id, payload){ return (await api.put(`/tournaments/${id}`, payload)).data }
export async function deleteTournament(id){ return (await api.delete(`/tournaments/${id}`)).data }
