import api from './apiClient.js'

export async function listPlayers(){ return (await api.get('/players')).data }
export async function createPlayer(payload){ return (await api.post('/players', payload)).data }
export async function updatePlayer(id, payload){ return (await api.put(`/players/${id}`, payload)).data }
export async function deletePlayer(id){ return (await api.delete(`/players/${id}`)).data }
