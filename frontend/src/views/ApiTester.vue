<template>
  <div class="api-tester">
    <div class="page-header">
      <h2>API Tester</h2>
    </div>

    <div class="request-section">
      <div class="request-bar">
        <select v-model="method" class="method-select">
          <option>GET</option>
          <option>POST</option>
          <option>PUT</option>
          <option>DELETE</option>
        </select>
        <input v-model="url" type="text" placeholder="/mock/users" class="url-input" />
        <button class="btn-send" @click="send" :disabled="sending">
          {{ sending ? 'Sending...' : 'Send' }}
        </button>
      </div>

      <div v-if="method === 'POST' || method === 'PUT'" class="request-body-section">
        <label>Request Body (JSON)</label>
        <textarea v-model="requestBody" rows="6" placeholder='{"key": "value"}'></textarea>
      </div>
    </div>

    <div v-if="response" class="response-section">
      <div class="response-header">
        <span class="response-status" :class="statusClass(response.status)">
          Status: {{ response.status }}
        </span>
        <span class="response-time">{{ responseTime }}ms</span>
      </div>
      <pre class="response-body">{{ formatJson(response.data) }}</pre>
    </div>

    <div v-if="error" class="error-msg">{{ error }}</div>
  </div>
</template>

<script>
import axios from 'axios'

export default {
  name: 'ApiTester',
  data() {
    return {
      method: 'GET',
      url: '/mock/users',
      requestBody: '{\n  "email": "user@example.com",\n  "name": "Chloe"\n}',
      response: null,
      responseTime: 0,
      sending: false,
      error: ''
    }
  },
  methods: {
    async send() {
      this.error = ''
      this.response = null
      this.sending = true

      const start = performance.now()

      try {
        const config = {
          method: this.method,
          url: fullUrl,
          baseURL: undefined,
          validateStatus: () => true
        }

        if ((this.method === 'POST' || this.method === 'PUT') && this.requestBody.trim()) {
          try {
            config.data = JSON.parse(this.requestBody)
          } catch {
            this.error = 'Request body is not valid JSON'
            this.sending = false
            return
          }
        }

        const res = await axios(config)
        this.response = { status: res.status, data: res.data }
      } catch (e) {
        this.response = { status: 0, data: { error: 'Network error' } }
      } finally {
        this.responseTime = Math.round(performance.now() - start)
        this.sending = false
      }
    },
    formatJson(obj) {
      return JSON.stringify(obj, null, 2)
    },
    statusClass(s) {
      if (s < 300) return 'status-2xx'
      if (s < 400) return 'status-3xx'
      if (s < 500) return 'status-4xx'
      return 'status-5xx'
    }
  }
}
</script>

<style scoped>
.page-header { margin-bottom: 24px; }
.page-header h2 { font-size: 20px; font-weight: 600; color: #1e293b; margin: 0; }
.request-section { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; margin-bottom: 16px; }
.request-bar { display: flex; gap: 10px; align-items: stretch; }
.method-select { width: 100px; padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; font-weight: 600; background: #fff; cursor: pointer; }
.url-input { flex: 1; padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; font-family: 'SF Mono', 'Menlo', monospace; }
.url-input:focus, .method-select:focus { outline: none; border-color: #6366f1; }
.btn-send { padding: 8px 24px; background: #6366f1; color: #fff; border: none; border-radius: 6px; font-size: 14px; font-weight: 500; cursor: pointer; }
.btn-send:hover { background: #4f46e5; }
.btn-send:disabled { opacity: 0.6; }
.request-body-section { margin-top: 16px; }
.request-body-section label { display: block; font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 6px; }
.request-body-section textarea { width: 100%; padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-family: 'SF Mono', 'Menlo', monospace; font-size: 13px; resize: vertical; box-sizing: border-box; }
.response-section { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; }
.response-header { display: flex; justify-content: space-between; align-items: center; padding: 12px 20px; background: #f8fafc; border-bottom: 1px solid #e2e8f0; }
.response-status { font-size: 14px; font-weight: 600; }
.response-time { font-size: 13px; color: #64748b; font-family: 'SF Mono', 'Menlo', monospace; }
.status-2xx { color: #16a34a; }
.status-3xx { color: #d97706; }
.status-4xx { color: #dc2626; }
.status-5xx { color: #7c3aed; }
.response-body { padding: 20px; margin: 0; font-family: 'SF Mono', 'Menlo', monospace; font-size: 13px; color: #1e293b; overflow-x: auto; white-space: pre-wrap; }
.error-msg { padding: 12px; background: #fef2f2; border-radius: 6px; color: #dc2626; font-size: 13px; }
</style>