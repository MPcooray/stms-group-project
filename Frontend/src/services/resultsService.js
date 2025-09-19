import axios from "axios";

export async function getEventResults(eventId) {
  // Assumes backend endpoint: /api/events/{eventId}/results
  const res = await axios.get(`/api/events/${eventId}/results`);
  return Array.isArray(res.data) ? res.data : [];
}
