import { createRouter, createWebHistory } from 'vue-router'
import Dashboard from '../views/Dashboard.vue'
import MockApis from '../views/MockApis.vue'
import MockApiEdit from '../views/MockApiEdit.vue'
import ApiTester from '../views/ApiTester.vue'
import Logs from '../views/Logs.vue'

const routes = [
  { path: '/', name: 'Dashboard', component: Dashboard },
  { path: '/mock-apis', name: 'MockApis', component: MockApis },
  { path: '/mock-apis/new', name: 'MockApiCreate', component: MockApiEdit },
  { path: '/mock-apis/:id/edit', name: 'MockApiEdit', component: MockApiEdit },
  { path: '/api-tester', name: 'ApiTester', component: ApiTester },
  { path: '/logs', name: 'Logs', component: Logs }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router