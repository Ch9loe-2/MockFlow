import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' }
})

export default {
  // Dashboard
  getDashboard() {
    return api.get('/logs/dashboard')
  },

  // Mock APIs CRUD
  getMockApis() {
    return api.get('/mock-apis')
  },

  getMockApi(id) {
    return api.get(`/mock-apis/${id}`)
  },

  createMockApi(data) {
    return api.post('/mock-apis', data)
  },

  updateMockApi(id, data) {
    return api.put(`/mock-apis/${id}`, data)
  },

  deleteMockApi(id) {
    return api.delete(`/mock-apis/${id}`)
  },

  // Logs
  getLogs(params) {
    return api.get('/logs', { params })
  },

  clearLogs() {
    return api.delete('/logs')
  }
}