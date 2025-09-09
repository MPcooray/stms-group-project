import api from "./apiClient.js"

// Map backend -> UI shape
function mapFromApi(t) {
  return {
    id: t.id,
    name: t.name,
    location: t.venue, // backend: Venue
  startDate: t.date,
  endDate: t.endDate || undefined,
  }
}

// Map UI -> backend UpsertDto
function mapToApi(payload) {
  return {
    name: payload.name,
    venue: payload.location,
  date: payload.startDate || payload.date || new Date().toISOString().substring(0, 10),
  endDate: payload.endDate || null,
  }
}

export async function listTournaments() {
  try {
    const response = await api.get("/api/tournaments")
    const arr = Array.isArray(response.data) ? response.data : []
    return arr.map(mapFromApi)
  } catch (error) {
    console.error("Failed to fetch tournaments:", error)
    throw error
  }
}

export async function createTournament(payload) {
  try {
    if (!payload?.name || !payload?.location) {
      throw new Error("Tournament name and location are required")
    }
    const body = mapToApi(payload)
    const response = await api.post("/api/tournaments", body)
    return mapFromApi(response.data)
  } catch (error) {
    console.error("Failed to create tournament:", error)
    throw error
  }
}

export async function updateTournament(id, payload) {
  try {
    if (!payload?.name || !payload?.location) {
      throw new Error("Tournament name and location are required")
    }
    const body = mapToApi(payload)
    const response = await api.put(`/api/tournaments/${id}`, body)
    return response.data
  } catch (error) {
    console.error("Failed to update tournament:", error)
    throw error
  }
}

export async function deleteTournament(id) {
  try {
    const response = await api.delete(`/api/tournaments/${id}`)
    return response.data
  } catch (error) {
    console.error("Failed to delete tournament:", error)
    throw error
  }
}
