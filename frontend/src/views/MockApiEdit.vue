<template>
  <div class="mock-api-edit">
    <div class="page-header">
      <h2>{{ isEdit ? 'Edit Mock API' : 'New Mock API' }}</h2>
    </div>

    <div class="form-card">
      <div v-if="error" class="error-msg">{{ error }}</div>

      <div class="form-group">
        <label>Name</label>
        <input v-model="form.name" type="text" placeholder="e.g. Users List" />
      </div>

      <div class="form-row">
        <div class="form-group">
          <label>Method</label>
          <select v-model="form.method">
            <option>GET</option>
            <option>POST</option>
            <option>PUT</option>
            <option>DELETE</option>
          </select>
        </div>
        <div class="form-group">
          <label>Path</label>
          <input v-model="form.path" type="text" placeholder="/mock/users" />
        </div>
        <div class="form-group">
          <label>Status Code</label>
          <input v-model.number="form.statusCode" type="number" min="100" max="599" />
        </div>
      </div>

      <div class="form-group">
        <label>Response Body (JSON)</label>
        <textarea v-model="form.responseBody" rows="12" :class="{ 'input-error': jsonError }"></textarea>
        <span v-if="jsonError" class="field-error">{{ jsonError }}</span>
      </div>

      <div class="form-group">
        <label>Description</label>
        <input v-model="form.description" type="text" placeholder="Optional description" />
      </div>

      <div class="form-group">
        <label class="checkbox-label">
          <input type="checkbox" v-model="form.isEnabled" />
          Enabled
        </label>
      </div>

      <div class="form-actions">
        <button class="btn-primary" @click="save" :disabled="saving">
          {{ saving ? 'Saving...' : 'Save' }}
        </button>
        <button class="btn-secondary" @click="$router.push('/mock-apis')">Cancel</button>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../api'

export default {
  name: 'MockApiEdit',
  data() {
    return {
      isEdit: false,
      form: {
        name: '',
        method: 'GET',
        path: '',
        statusCode: 200,
        responseBody: '{\n  "code": 200,\n  "message": "success",\n  "data": []\n}',
        description: '',
        isEnabled: true
      },
      error: '',
      jsonError: '',
      saving: false
    }
  },
  async mounted() {
    const id = this.$route.params.id
    if (id) {
      this.isEdit = true
      try {
        const res = await api.getMockApi(id)
        const data = res.data
        this.form = {
          name: data.name,
          method: data.method,
          path: data.path,
          statusCode: data.statusCode,
          responseBody: data.responseBody,
          description: data.description,
          isEnabled: data.isEnabled
        }
      } catch (e) {
        this.error = 'Failed to load mock API'
      }
    }
  },
  methods: {
    validateJson() {
      try {
        JSON.parse(this.form.responseBody)
        this.jsonError = ''
        return true
      } catch (e) {
        this.jsonError = 'Response JSON format error'
        return false
      }
    },
    async save() {
      this.error = ''
      if (!this.validateJson()) return
      if (!this.form.name.trim() || !this.form.path.trim()) {
        this.error = 'Name and Path are required'
        return
      }
      this.saving = true
      try {
        if (this.isEdit) {
          await api.updateMockApi(this.$route.params.id, this.form)
        } else {
          await api.createMockApi(this.form)
        }
        this.$router.push('/mock-apis')
      } catch (e) {
        const msg = e.response?.data?.message || 'Save failed'
        this.error = msg
      } finally {
        this.saving = false
      }
    }
  }
}
</script>

<style scoped>
.page-header { margin-bottom: 24px; }
.page-header h2 { font-size: 20px; font-weight: 600; color: #1e293b; margin: 0; }
.form-card { background: #fff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 24px; max-width: 720px; }
.form-group { margin-bottom: 20px; }
.form-group label { display: block; font-size: 13px; font-weight: 600; color: #475569; margin-bottom: 6px; }
.form-row { display: grid; grid-template-columns: 1fr 2fr 1fr; gap: 16px; }
input[type="text"], input[type="number"], select, textarea {
  width: 100%; padding: 8px 12px; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; color: #1e293b; background: #fff; box-sizing: border-box;
}
input:focus, select:focus, textarea:focus { outline: none; border-color: #6366f1; box-shadow: 0 0 0 2px rgba(99,102,241,0.1); }
textarea { font-family: 'SF Mono', 'Menlo', monospace; font-size: 13px; resize: vertical; }
.input-error { border-color: #dc2626 !important; }
.field-error { display: block; margin-top: 4px; font-size: 12px; color: #dc2626; }
.checkbox-label { display: flex; align-items: center; gap: 8px; font-size: 14px; color: #1e293b; cursor: pointer; }
.checkbox-label input { width: auto; }
.form-actions { display: flex; gap: 12px; padding-top: 8px; }
.btn-primary { padding: 10px 24px; background: #6366f1; color: #fff; border: none; border-radius: 6px; font-size: 14px; font-weight: 500; cursor: pointer; }
.btn-primary:hover { background: #4f46e5; }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }
.btn-secondary { padding: 10px 24px; background: #fff; color: #475569; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 14px; cursor: pointer; }
.btn-secondary:hover { background: #f8fafc; }
.error-msg { margin-bottom: 16px; padding: 12px; background: #fef2f2; border-radius: 6px; color: #dc2626; font-size: 13px; }
</style>