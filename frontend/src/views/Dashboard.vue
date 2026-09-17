<template>
  <div class="dashboard">
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-label">Mock APIs</div>
        <div class="stat-value">{{ stats.totalApis }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Enabled</div>
        <div class="stat-value">{{ stats.enabledApis }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Total Requests</div>
        <div class="stat-value">{{ stats.totalRequests }}</div>
      </div>
      <div class="stat-card">
        <div class="stat-label">Avg Response</div>
        <div class="stat-value">{{ stats.averageResponseTimeMs }}ms</div>
      </div>
    </div>

    <div class="dashboard-row">
      <div class="dashboard-card chart-card">
        <div class="card-header">Daily Requests (Last 7 Days)</div>
        <div class="chart-container" v-if="stats.dailyRequests && stats.dailyRequests.length">
          <div class="bar-chart">
            <div
              v-for="item in stats.dailyRequests"
              :key="item.date"
              class="bar-column"
            >
              <div
                class="bar"
                :style="{ height: barHeight(item.count) + '%' }"
                :title="item.date + ': ' + item.count + ' requests'"
              ></div>
              <div class="bar-label">{{ item.date }}</div>
            </div>
          </div>
        </div>
        <div v-else class="chart-empty">No request data yet</div>
      </div>

      <div class="dashboard-card">
        <div class="card-header">Recent Requests</div>
        <div v-if="stats.recentLogs && stats.recentLogs.length" class="log-list">
          <div v-for="log in stats.recentLogs" :key="log.id" class="log-item">
            <span class="log-method" :class="methodClass(log.method)">{{ log.method }}</span>
            <span class="log-path">{{ log.path }}</span>
            <span class="log-status" :class="statusClass(log.statusCode)">{{ log.statusCode }}</span>
            <span class="log-time">{{ log.responseTimeMs }}ms</span>
          </div>
        </div>
        <div v-else class="chart-empty">No requests recorded yet</div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../api'

export default {
  name: 'Dashboard',
  data() {
    return {
      stats: {}
    }
  },
  mounted() {
    this.fetchData()
  },
  methods: {
    async fetchData() {
      try {
        const res = await api.getDashboard()
        this.stats = res.data
      } catch (e) {
        console.error('Failed to load dashboard:', e)
      }
    },
    methodClass(m) {
      return { get: m === 'GET', post: m === 'POST', put: m === 'PUT', delete: m === 'DELETE' }
    },
    statusClass(s) {
      if (s < 300) return 'status-2xx'
      if (s < 400) return 'status-3xx'
      if (s < 500) return 'status-4xx'
      return 'status-5xx'
    },
    barHeight(count) {
      const max = Math.max(...this.stats.dailyRequests.map(d => d.count), 1)
      return (count / max) * 100
    }
  }
}
</script>

<style scoped>
.dashboard { padding: 0; }
.stats-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; margin-bottom: 24px; }
.stat-card { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; }
.stat-label { font-size: 13px; color: #64748b; margin-bottom: 8px; }
.stat-value { font-size: 28px; font-weight: 600; color: #1e293b; }
.dashboard-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.dashboard-card { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 20px; }
.card-header { font-size: 14px; font-weight: 600; color: #475569; margin-bottom: 16px; padding-bottom: 12px; border-bottom: 1px solid #f1f5f9; }
.chart-container { height: 180px; }
.bar-chart { display: flex; align-items: flex-end; height: 100%; gap: 8px; }
.bar-column { flex: 1; display: flex; flex-direction: column; align-items: center; height: 100%; }
.bar { width: 100%; max-width: 40px; background: #6366f1; border-radius: 4px 4px 0 0; min-height: 4px; transition: height 0.3s; }
.bar-label { font-size: 11px; color: #94a3b8; margin-top: 6px; }
.chart-empty { color: #94a3b8; font-size: 14px; padding: 40px 0; text-align: center; }
.log-list { display: flex; flex-direction: column; gap: 8px; }
.log-item { display: flex; align-items: center; gap: 12px; padding: 8px 0; border-bottom: 1px solid #f8fafc; font-size: 13px; font-family: 'SF Mono', 'Menlo', monospace; }
.log-method { display: inline-block; width: 48px; padding: 2px 6px; border-radius: 4px; text-align: center; font-weight: 600; font-size: 11px; }
.log-method.get { background: #dbeafe; color: #2563eb; }
.log-method.post { background: #dcfce7; color: #16a34a; }
.log-method.put { background: #fef3c7; color: #d97706; }
.log-method.delete { background: #fee2e2; color: #dc2626; }
.log-path { flex: 1; color: #334155; }
.log-status { font-weight: 600; width: 36px; text-align: right; }
.status-2xx { color: #16a34a; }
.status-3xx { color: #d97706; }
.status-4xx { color: #dc2626; }
.status-5xx { color: #7c3aed; }
.log-time { color: #94a3b8; width: 48px; text-align: right; }
</style>