import axios from "axios";

export async function getEventResults(eventId, gender) {
  // Assumes backend endpoint: /api/events/{eventId}/results
  const params = {};
  if (gender && gender !== 'All') params.gender = gender;
  const res = await axios.get(`/api/events/${eventId}/results`, { params });
  return Array.isArray(res.data) ? res.data : [];
}
