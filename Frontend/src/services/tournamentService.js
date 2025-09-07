import api from "./apiClient.js"

export async function listTournaments() {
  try {
    const response = await api.get("/tournaments")
    return response.data
  } catch (error) {
    console.error("Failed to fetch tournaments:", error)
    throw error
  }
}

export async function createTournament(payload) {
  try {
    if (!payload.name || !payload.location) {
      throw new Error("Tournament name and location are required")
    }
    const response = await api.post("/tournaments", payload)
    return response.data
  } catch (error) {
    console.error("Failed to create tournament:", error)
    throw error
  }
}

export async function updateTournament(id, payload) {
  try {
    if (!payload.name || !payload.location) {
      throw new Error("Tournament name and location are required")
    }
    const response = await api.put(`/tournaments/${id}`, payload)
    return response.data
  } catch (error) {
    console.error("Failed to update tournament:", error)
    throw error
  }
}

export async function deleteTournament(id) {
  try {
    const response = await api.delete(`/tournaments/${id}`)
    return response.data
  } catch (error) {
    console.error("Failed to delete tournament:", error)
    throw error
  }
}
