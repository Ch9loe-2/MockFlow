<template>
  <div class="app-layout">
    <aside class="sidebar">
      <div class="logo">
        <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2"><path d="M12 2L2 7l10 5 10-5-10-5z"/><path d="M2 17l10 5 10-5"/><path d="M2 12l10 5 10-5"/></svg>
        <span>MockFlow</span>
      </div>
      <nav class="nav">
        <router-link to="/" class="nav-item" exact-active-class="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="7" height="9"/><rect x="14" y="3" width="7" height="5"/><rect x="14" y="12" width="7" height="9"/><rect x="3" y="16" width="7" height="5"/></svg>
          Dashboard
        </router-link>
        <router-link to="/mock-apis" class="nav-item" active-class="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>
          Mock APIs
        </router-link>
        <router-link to="/api-tester" class="nav-item" active-class="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          API Tester
        </router-link>
        <router-link to="/logs" class="nav-item" active-class="active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/></svg>
          Logs
        </router-link>
      </nav>
    </aside>
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<script>
export default { name: 'App' }
</script>

<style>
* { margin: 0; padding: 0; box-sizing: border-box; }
body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background: #f1f5f9; color: #1e293b; }
.app-layout { display: flex; min-height: 100vh; }
.sidebar { width: 220px; background: #fff; border-right: 1px solid #e2e8f0; padding: 24px 16px; display: flex; flex-direction: column; position: fixed; top: 0; left: 0; bottom: 0; }
.logo { display: flex; align-items: center; gap: 10px; font-size: 18px; font-weight: 700; color: #1e293b; margin-bottom: 32px; padding: 0 8px; }
.logo svg { color: #6366f1; }
.nav { display: flex; flex-direction: column; gap: 4px; }
.nav-item { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border-radius: 6px; font-size: 14px; font-weight: 500; color: #64748b; text-decoration: none; transition: all 0.15s; }
.nav-item:hover { background: #f8fafc; color: #1e293b; }
.nav-item.active { background: #eef2ff; color: #4f46e5; }
.nav-item svg { flex-shrink: 0; }
.main-content { margin-left: 220px; flex: 1; padding: 32px; max-width: 1200px; }
</style>