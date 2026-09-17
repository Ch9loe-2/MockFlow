<template>
  <div class="logs-page">
    <div class="page-header">
      <h2>Request Logs</h2>
      <button class="btn-danger" @click="clearLogs">Clear All</button>
    </div>

    <div class="filters">
      <select v-model="filterMethod" @change="fetchLogs">
        <option value="">All Methods</option>
        <option>GET</option>
        <option>POST</option>
        <option>PUT</option>
        <option>DELETE</option>
      </select>
      <select v-model="filterStatus" @change="fetchLogs">
        <option value="">All Status Codes</option>
        <option value="200-299">2xx</option>
        <option value="400-499">4xx</option>
        <option value="500-599">5xx</option>
      </select>
    </div>

    <div v-if="loading" class="loading">Loading...</div>

    <div v-else-if="logs.length === 0" class="empty">No request logs yet</div>

    <div v-else class="log-table">
      <div class="log-header">
        <span class="l-time">Time</span>
        <span class="l-method">Method</span>
        <span class="l-path">Path</span>
        <span class="l-status">Status</span>
        <span class="l-ms">Duration</span>
      </div>
      <div v-for="log in logs" :key="log.id" class="log-row">
        <span class="l-time">{{ formatTime(log.requestedAt) }}</span>
        <span class="l-method">
          <span class="method-badge" :class="log.method.toLowerCase()">{{ log.method }}</span>
        </span>
        <span class="l-path code">{{ log.path }}</span>
        <span class="l-status" :class="statusClass(log.statusCode)">{{ log.statusCode }}</span>
        <span class="l-ms">{{ log.responseTimeMs }}ms</span>
      </div>
    </div>

    <div v-if="total > pageSize" class="pagination">
      <button :disabled="page <= 1" @click="prevPage" class="btn-prev">Previous</button>
      <span class="page-info">Page {{ page }} of {{ totalPages }}</span>
      <button :disabled="page >= totalPages" @click="nextPage" class="btn-next">Next</button>
    </div>

    <div v-if="error" class="error-msg">{{ error }}</div>
  </div>
</template>

<script>
import api from '../api'

export default {
  name: 'Logs',
  data() {
    return {
      logs: [],
      loading: true,
      error: '',
      filterMethod: '',
      filterStatus: '',
      page: 1,
      pageSize: 20,
      total: 0
    }
  },
  computed: {
    totalPages() {
      return Math.ceil(this.total / this.pageSize)
    }
  },
  mounted() {
    this.fetchLogs()
  },
  methods: {
    async fetchLogs() {
      try {
        this.loading = true
        const params = { page: this.page, pageSize: this.pageSize }
        if (this.filterMethod) params.method = this.filterMethod
        if (this.filterStatus) params.statusCodeRange = this.filterStatus

        const res = await api.getLogs(params)
        this.logs = res.data.items
        this.total = res.data.total
      } catch (e) {
        this.error = 'Failed to load logs'
      } finally {
        this.loading = false
      }
    },
    formatTime(dateStr) {
      const d = new Date(dateStr)
      const pad = n => String(n).padStart(2, '0')
      return pad(d.getHours()) + ':' + pad(d.getMinutes()) + ':' + pad(d.getSeconds())
    },
    statusClass(s) {
      if (s < 300) return 'status-2xx'
      if (s < 400) return 'status-3xx'
      if (s < 500) return 'status-4xx'
      return 'status-5xx'
    },
    async clearLogs() {
      if (!confirm('Clear all request logs?')) return
      try {
        await api.clearLogs()
        this.logs = []
        this.total = 0
      } catch (e) {
        this.error = 'Failed to clear logs'
      }
    },
    prevPage() {
      if (this.page > 1) { this.page--; this.fetchLogs() }
    },
    nextPage() {
      if (this.page < this.totalPages) { this.page++; this.fetchLogs() }
    }
  }
}
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.page-header h2 { font-size: 20px; font-weight: 600; color: #1e293b; margin: 0; }
.loading, .empty { color: #94a3b8; padding: 40px; text-align: center; }
.filters { display: flex; gap: 12px; margin-bottom: 16px; }
.filters select { padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 13px; background: #fff; cursor: pointer; }
.log-table { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; }
.log-header, .log-row { display: grid; grid-template-columns: 80px 80px 1fr 80px 80px; gap: 8px; padding: 10px 16px; align-items: center; }
.log-header { background: #f8fafc; font-size: 12px; font-weight: 600; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
.log-row { border-top: 1px solid #f1f5f9; font-size: 13px; }
.log-row:hover { background: #f8fafc; }
.code { font-family: 'SF Mono', 'Menlo', monospace; color: #475569; }
.method-badge { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 700; }
.method-badge.get { background: #dbeafe; color: #2563eb; }
.method-badge.post { background: #dcfce7; color: #16a34a; }
.method-badge.put { background: #fef3c7; color: #d97706; }
.method-badge.delete { background: #fee2e2; color: #dc2626; }
.status-2xx { color: #16a34a; font-weight: 600; }
.status-4xx { color: #dc2626; font-weight: 600; }
.status-5xx { color: #7c3aed; font-weight: 600; }
.l-ms { font-family: 'SF Mono', 'Menlo', monospace; color: #94a3b8; text-align: right; }
.pagination { display: flex; justify-content: center; align-items: center; gap: 16px; margin-top: 16px; }
.page-info { font-size: 13px; color: #64748b; }
.btn-prev, .btn-next { padding: 6px 16px; border: 1px solid #e2e8f0; border-radius: 4px; background: #fff; font-size: 13px; cursor: pointer; }
.btn-prev:disabled, .btn-next:disabled { opacity: 0.4; cursor: not-allowed; }
.btn-danger { padding: 8px 16px; background: #fff; color: #dc2626; border: 1px solid #fecaca; border-radius: 6px; font-size: 13px; cursor: pointer; }
.btn-danger:hover { background: #fef2f2; }
.error-msg { margin-top: 12px; padding: 12px; background: #fef2f2; border-radius: 6px; color: #dc2626; font-size: 13px; }
</style>