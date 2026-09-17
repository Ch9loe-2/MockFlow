<template>
  <div class="mock-apis">
    <div class="page-header">
      <h2>Mock APIs</h2>
      <router-link to="/mock-apis/new" class="btn-primary">+ New Mock API</router-link>
    </div>

    <div v-if="loading" class="loading">Loading...</div>

    <div v-else-if="apis.length === 0" class="empty">
      No mock APIs yet. <router-link to="/mock-apis/new">Create one</router-link>
    </div>

    <div v-else class="api-table">
      <div class="table-header">
        <span class="col-name">Name</span>
        <span class="col-method">Method</span>
        <span class="col-path">Path</span>
        <span class="col-status">Status</span>
        <span class="col-enabled">State</span>
        <span class="col-actions">Actions</span>
      </div>
      <div v-for="api in apis" :key="api.id" class="table-row">
        <span class="col-name">{{ api.name }}</span>
        <span class="col-method">
          <span class="method-badge" :class="methodClass(api.method)">{{ api.method }}</span>
        </span>
        <span class="col-path code">{{ api.path }}</span>
        <span class="col-status">{{ api.statusCode }}</span>
        <span class="col-enabled">
          <span class="state-dot" :class="api.isEnabled ? 'enabled' : 'disabled'"></span>
          {{ api.isEnabled ? 'Enabled' : 'Disabled' }}
        </span>
        <span class="col-actions">
          <button class="btn-sm" @click="editApi(api.id)">Edit</button>
          <button class="btn-sm btn-danger" @click="deleteApi(api)">Delete</button>
        </span>
      </div>
    </div>

    <div v-if="error" class="error-msg">{{ error }}</div>
  </div>
</template>

<script>
import api from '../api'

export default {
  name: 'MockApis',
  data() {
    return { apis: [], loading: true, error: '' }
  },
  mounted() {
    this.fetchApis()
  },
  methods: {
    async fetchApis() {
      try {
        this.loading = true
        const res = await api.getMockApis()
        this.apis = res.data
      } catch (e) {
        this.error = 'Failed to load mock APIs'
      } finally {
        this.loading = false
      }
    },
    methodClass(m) {
      return m.toLowerCase()
    },
    editApi(id) {
      this.$router.push('/mock-apis/' + id + '/edit')
    },
    async deleteApi(item) {
      if (!confirm('Delete "' + item.name + '"?')) return
      try {
        await api.deleteMockApi(item.id)
        this.apis = this.apis.filter(a => a.id !== item.id)
      } catch (e) {
        this.error = 'Failed to delete'
      }
    }
  }
}
</script>

<style scoped>
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.page-header h2 { font-size: 20px; font-weight: 600; color: #1e293b; margin: 0; }
.loading, .empty { color: #94a3b8; padding: 40px; text-align: center; }
.error-msg { margin-top: 12px; padding: 12px; background: #fef2f2; border-radius: 6px; color: #dc2626; font-size: 13px; }
.api-table { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; overflow: hidden; }
.table-header { display: grid; grid-template-columns: 2fr 80px 1.5fr 80px 100px 120px; gap: 8px; padding: 12px 16px; background: #f8fafc; font-size: 12px; font-weight: 600; color: #64748b; text-transform: uppercase; letter-spacing: 0.5px; }
.table-row { display: grid; grid-template-columns: 2fr 80px 1.5fr 80px 100px 120px; gap: 8px; padding: 12px 16px; border-top: 1px solid #f1f5f9; align-items: center; font-size: 14px; }
.table-row:hover { background: #f8fafc; }
.col-name { color: #1e293b; font-weight: 500; }
.code { font-family: 'SF Mono', 'Menlo', monospace; font-size: 13px; color: #475569; }
.method-badge { display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 700; }
.method-badge.get { background: #dbeafe; color: #2563eb; }
.method-badge.post { background: #dcfce7; color: #16a34a; }
.method-badge.put { background: #fef3c7; color: #d97706; }
.method-badge.delete { background: #fee2e2; color: #dc2626; }
.state-dot { display: inline-block; width: 8px; height: 8px; border-radius: 50%; margin-right: 6px; }
.state-dot.enabled { background: #22c55e; }
.state-dot.disabled { background: #cbd5e1; }
.btn-primary { display: inline-block; padding: 8px 16px; background: #6366f1; color: #fff; border: none; border-radius: 6px; font-size: 13px; font-weight: 500; cursor: pointer; text-decoration: none; }
.btn-sm { padding: 4px 12px; border: 1px solid #e2e8f0; border-radius: 4px; background: #fff; color: #475569; font-size: 12px; cursor: pointer; margin-right: 6px; }
.btn-sm:hover { background: #f8fafc; }
.btn-danger { color: #dc2626; border-color: #fecaca; }
.btn-danger:hover { background: #fef2f2; }
.btn-primary:hover { background: #4f46e5; }
</style>