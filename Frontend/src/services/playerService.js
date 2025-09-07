import api from "./apiClient.js"

export async function listPlayers() {
  try {
    const response = await api.get("/players")
    return response.data
  } catch (error) {
    console.error("Failed to fetch players:", error)
    throw error
  }
}

export async function createPlayer(payload) {
  try {
    if (!payload.name || !payload.university) {
      throw new Error("Player name and university are required")
    }
    if (payload.age && (payload.age < 1 || payload.age > 100)) {
      throw new Error("Age must be between 1 and 100")
    }
    const response = await api.post("/players", payload)
    return response.data
  } catch (error) {
    console.error("Failed to create player:", error)
    throw error
  }
}

export async function updatePlayer(id, payload) {
  try {
    if (!payload.name || !payload.university) {
      throw new Error("Player name and university are required")
    }
    if (payload.age && (payload.age < 1 || payload.age > 100)) {
      throw new Error("Age must be between 1 and 100")
    }
    const response = await api.put(`/players/${id}`, payload)
    return response.data
  } catch (error) {
    console.error("Failed to update player:", error)
    throw error
  }
}

export async function deletePlayer(id) {
  try {
    const response = await api.delete(`/players/${id}`)
    return response.data
  } catch (error) {
    console.error("Failed to delete player:", error)
    throw error
  }
}
