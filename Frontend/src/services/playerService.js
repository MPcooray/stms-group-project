import api from "./apiClient.js"

// Listing is by university: GET /api/universities/{universityId}/players
export async function listPlayersByUniversity(universityId) {
  if (!universityId) throw new Error("universityId is required")
  try {
    const response = await api.get(`/api/universities/${universityId}/players`)
    return response.data
  } catch (error) {
    console.error("Failed to fetch players:", error)
    throw error
  }
}

// Create under a university: POST /api/universities/{universityId}/players
export async function createPlayer(universityId, payload) {
  if (!universityId) throw new Error("universityId is required")
  try {
    if (!payload?.name) {
      throw new Error("Player name is required")
    }
    if (payload.age && (payload.age < 1 || payload.age > 120)) {
      throw new Error("Age must be between 1 and 120")
    }
    if (payload.gender && !["Male", "Female"].includes(payload.gender)) {
      throw new Error("Gender must be Male or Female")
    }
    const body = { name: payload.name, age: payload.age ?? null, gender: payload.gender ?? null }
    const response = await api.post(`/api/universities/${universityId}/players`, body)
    return response.data
  } catch (error) {
    console.error("Failed to create player:", error)
    throw error
  }
}

export async function getPlayer(id) {
  try {
    const response = await api.get(`/api/players/${id}`)
    return response.data
  } catch (error) {
    console.error("Failed to fetch player:", error)
    throw error
  }
}

export async function updatePlayer(id, payload) {
  try {
    if (!payload?.name) {
      throw new Error("Player name is required")
    }
    if (payload.age && (payload.age < 1 || payload.age > 120)) {
      throw new Error("Age must be between 1 and 120")
    }
    if (payload.gender && !["Male", "Female"].includes(payload.gender)) {
      throw new Error("Gender must be Male or Female")
    }
    const body = { name: payload.name, age: payload.age ?? null, gender: payload.gender ?? null }
    const response = await api.put(`/api/players/${id}`, body)
    return response.data
  } catch (error) {
    console.error("Failed to update player:", error)
    throw error
  }
}

export async function deletePlayer(id) {
  try {
    const response = await api.delete(`/api/players/${id}`)
    return response.data
  } catch (error) {
    console.error("Failed to delete player:", error)
    throw error
  }
}
