# MockFlow — 可视化 API Mock 与接口调试平台

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![Vue](https://img.shields.io/badge/Vue-3.5-4FC08D)](https://vuejs.org/)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)

MockFlow 是一个轻量级的可视化 API Mock 平台 + 接口调试工具。开发者无需编写任何代码，即可在 Web 界面中创建和管理 Mock API，并支持其他客户端（浏览器、curl、Postman 等）直接访问。同时内置简化版 Postman，方便快速调试。

---

## 项目背景

在日常前后端分离开发中，前端经常因为后端接口未就绪而阻塞开发。MockFlow 旨在解决这一痛点：

- **零代码创建 Mock**：Web 界面配置，即时生效
- **一站式管理**：所有 Mock 接口集中管理，启停可控
- **请求追踪**：记录所有请求日志，方便调试和分析
- **即用即调**：内置 API Tester，无需切换到 Postman

---

## 核心功能

### 📊 Dashboard 仪表盘
- 总览 Mock API 数量、启用数、请求总量、平均响应时间
- 近 7 天请求量趋势图
- 最近请求日志实时展示

### 🔧 Mock API 管理
- 创建/编辑/删除 Mock API
- 支持 GET / POST / PUT / DELETE 方法
- 启用/禁用控制
- 精确配置 Status Code 和 Response Body
- JSON 格式前端校验，防止保存无效数据

### ⚡ 动态 Mock 响应
- 配置即生效，无需重启服务
- 支持不同 Method + Path 的组合
- 不存在的 API → 404
- 被禁用的 API → 403
- 统一 JSON 错误响应格式

### 🔍 API Tester
- 简化版 Postman，直接输入 URL 和 Method 发送请求
- 支持 GET / POST / PUT / DELETE
- JSON Request Body 编辑
- 实时显示 Response Status、Body、响应时间

### 📝 请求日志
- 自动记录每次 Mock 请求
- Method / Status Code 筛选
- 分页查看
- 一键清空日志

---

## 技术栈

| 层级 | 技术 |
|------|------|
| **前端** | Vue 3 (Options API) + Vite 8 + Vue Router 4 + Axios |
| **后端** | C# / ASP.NET Core 10 + Entity Framework Core 10 |
| **数据库** | SQLite |
| **架构** | 前后端分离（Vite Proxy 代理） |
| **关键设计** | 自定义 ASP.NET Core Middleware 实现动态 Mock 路由 |

---

## 系统架构

```
┌─────────────────────────────────────────────────────────────────┐
│  Browser (Vue 3 SPA)                                           │
│  ┌──────────┬───────────┬──────────┬─────────┐                 │
│  │Dashboard │Mock APIs  │API Tester│  Logs   │                 │
│  └────┬─────┴─────┬─────┴────┬────┴────┬────┘                 │
│       │           │          │         │                        │
│       └───────────┴──────────┴─────────┘                        │
│                         │ Axios                                 │
│                    Vite Dev Proxy (:5173)                       │
└─────────────────────────┬───────────────────────────────────────┘
                          │
┌─────────────────────────▼───────────────────────────────────────┐
│  ASP.NET Core (:5001)                                           │
│  ┌─────────────┐  ┌──────────────────┐  ┌──────────────────┐   │
│  │ Controllers │  │ExceptionMiddleware│  │MockMiddleware    │   │
│  │ MockApis    │  │  (全局异常处理)    │  │ (动态Mock路由)   │   │
│  │ Logs        │  └──────────────────┘  └──────────────────┘   │
│  └──────┬──────┘                                                │
│         │                                                       │
│  ┌──────▼──────┐                                                │
│  │  Services   │                                                │
│  │ MockApiSvc  │  RequestLogSvc                                 │
│  └──────┬──────┘                                                │
│         │                                                       │
│  ┌──────▼──────────┐                                            │
│  │ EF Core + SQLite│                                            │
│  │ MockApis Table  │  RequestLogs Table                         │
│  └─────────────────┘                                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## 数据库设计

### MockApis 表

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | INTEGER PK | 主键 |
| Name | TEXT | API 名称 |
| Method | TEXT | HTTP 方法 (GET/POST/PUT/DELETE) |
| Path | TEXT | 请求路径，如 /mock/users |
| StatusCode | INTEGER | 返回的 HTTP 状态码 |
| ResponseBody | TEXT | 返回的 JSON 响应体 |
| Description | TEXT | 描述 |
| IsEnabled | INTEGER (bool) | 是否启用 |
| CreatedAt | DATETIME | 创建时间 |
| UpdatedAt | DATETIME | 更新时间 |

⚠️ **Method + Path 联合唯一索引**：不允许创建相同 Method 和 Path 的重复配置

### RequestLogs 表

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | INTEGER PK | 主键 |
| MockApiId | INTEGER FK | 关联的 Mock API（可为 null，404 请求无关联） |
| Method | TEXT | 请求方法 |
| Path | TEXT | 请求路径 |
| StatusCode | INTEGER | 响应状态码 |
| ResponseTimeMs | INTEGER | 响应时间（毫秒） |
| RequestedAt | DATETIME | 请求时间 |

---

## API 说明

### Mock API 管理

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/mock-apis` | 获取所有 Mock API |
| GET | `/api/mock-apis/{id}` | 获取单个 Mock API |
| POST | `/api/mock-apis` | 创建 Mock API |
| PUT | `/api/mock-apis/{id}` | 更新 Mock API |
| DELETE | `/api/mock-apis/{id}` | 删除 Mock API |

### 动态 Mock 接口

| 方法 | 路径 | 说明 |
|------|------|------|
| * | `/mock/{path}` | 动态匹配数据库中的 Mock 配置，返回配置的 JSON |

### 请求日志

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | `/api/logs` | 获取日志（支持 method/statusCode 筛选、分页） |
| DELETE | `/api/logs` | 清空所有日志 |
| GET | `/api/logs/dashboard` | 获取 Dashboard 统计数据 |

---

## 项目启动

### 前置条件

- .NET 10 SDK
- Node.js 22+
- npm 10+

### 启动后端

```bash
cd MockFlow/backend
dotnet run
```

启动后监听 `http://localhost:5001`。首次启动自动创建 SQLite 数据库并写入 4 个示例 Mock API。

### 启动前端

```bash
cd MockFlow/frontend
npm install
npm run dev
```

启动后默认监听 `http://localhost:5173`，Vite 自动代理 `/api` 和 `/mock` 请求到后端 5001 端口。

### 访问

浏览器打开 `http://localhost:5173`（或 Vite 分配的其他端口）即可使用管理后台。

---

## 使用示例

### 1. 创建 Mock API

```json
// POST /api/mock-apis
{
  "name": "Get User",
  "method": "GET",
  "path": "/mock/users",
  "statusCode": 200,
  "responseBody": "{\"code\":200,\"message\":\"success\",\"data\":[{\"id\":1,\"name\":\"Chloe\"}]}",
  "description": "Returns user list",
  "isEnabled": true
}
```

### 2. 访问 Mock 接口

```bash
curl http://localhost:5001/mock/users
```

响应：

```json
{
  "code": 200,
  "message": "success",
  "data": [
    { "id": 1, "name": "Chloe" }
  ]
}
```

### 3. 查看请求日志

```bash
curl http://localhost:5001/api/logs
```

### 4. Dashboard 统计

```bash
curl http://localhost:5001/api/logs/dashboard
```

### 5. 不存在的 Mock → 404

```bash
curl http://localhost:5001/mock/does-not-exist
# {"code":404,"message":"Mock API not found"}
```

### 6. 禁用的 Mock → 403

```bash
# 先禁用某个 Mock API，再访问
curl http://localhost:5001/mock/disabled-endpoint
# {"code":403,"message":"Mock API is disabled"}
```

---

## 第二轮深度 Code Review（2026-09-17）

### 🔴 发现并修复的关键 Bug

| # | 严重程度 | Bug 描述 | 修复方式 |
|---|---------|----------|---------|
| 1 | **P0（编译/运行时错误）** | `ApiTester.vue` 中 `fullUrl` 变量未定义：之前清理死代码时删除了变量定义但未删除引用，导致 Axios 请求的 `url` 指向 `undefined`，API Tester 功能不可用 | 将 `url: fullUrl` 替换为 `url: this.url` |
| 2 | **P1（功能缺陷）** | Logs 页面 Status Code 筛选器使用 `value="400"`，只精确匹配 400 状态码，无法筛选整个 4xx 范围。用户选"4xx"期望看到所有 4xx 错误（400/403/404/409等） | 改为 `statusCodeRange=400-499` 范围匹配，后端同时兼容精确 `statusCode` 参数和 `statusCodeRange` 范围参数 |
| 3 | **P1（代码质量）** | `MockApiService` 中 `FindDisabledMockAsync()` 方法已无调用方（上一轮优化已用 `FindMockIgnoreEnabledAsync` 替代），成为死代码 | 删除该方法 |
| 4 | **P1（安全加固）** | 后端 Controller 层缺少输入验证：前端有 Name/Path 非空验证，但后端接口（curl/Postman 调用）可以绕过前端验证直接写入空值或超长数据 | 在 POST 和 PUT 端增加 Name/Path 非空、长度上限（Name 100、Path 500）和 Method 白名单校验 |
| 5 | **P2（代码重复）** | `CreateMockApiDto` 和 `UpdateMockApiDto` 字段定义完全一致，6 个属性重复声明 | 提取 `MockApiBaseDto` 抽象基类，两个 DTO 继承基类，减少 ~50% 重复代码 |
| 6 | **P2（生产安全）** | `ExceptionMiddleware` 在生产环境会静默输出 500，排障困难；同时使用匿名对象，无法区分开发/生产环境 | 注入 `IHostEnvironment`，开发环境返回 `detail` + `stackTrace`，生产环境仅返回安全错误信息 |

### 测试回归

第二轮修复后，62 项自动化测试全部通过，新增验证：
- `statusCodeRange=200-299` 范围过滤 ✅
- `statusCodeRange=400-499` 范围过滤 ✅
- 后端字段空值校验 ✅
- 无效 Method 拒绝 ✅

---

## 技术难点

1. **动态路由匹配**：使用 ASP.NET Core Middleware 处理所有 `/mock/*` 请求，在运行时根据数据库配置动态生成响应，而非手写 Controller 路由。中间件中同时记录请求日志和响应时间。

2. **Method + Path 唯一约束**：通过 EF Core 的联合唯一索引确保同一个 Method + Path 组合不会重复配置，CREATE 操作捕获并处理 SQLite 的唯一约束冲突异常。

3. **前后端联调代理**：Vite Dev Server 自动代理 `/api` 和 `/mock` 路径到后端，开发时前端不需要处理跨域问题。生产部署时可配置 nginx 反向代理。

4. **SQLite 上 GroupBy 翻译**：EF Core 对 SQLite 的 `GroupBy` + `ToString` 组合翻译有限，Dashboard 的日请求统计通过先查询原始日期数据，再在内存中分组计算，避免翻译失败。

---

## 后续扩展方向

- [ ] **Mock API 分组**：按项目/模块组织 Mock 接口
- [ ] **动态响应逻辑**：支持条件响应（根据请求参数返回不同结果）
- [ ] **延迟模拟**：可配置响应延迟，模拟慢接口场景
- [ ] **请求体匹配**：根据 Request Body 内容返回不同响应
- [ ] **响应模板引擎**：支持模板变量（如 `{{timestamp}}`）
- [ ] **Curl 代码生成**：一键生成 curl 命令
- [ ] **历史请求回放**：从日志重新发送请求
- [ ] **多用户支持**：登录和权限管理
- [ ] **API 导入导出**：支持 OpenAPI / Swagger 导入

---

## 测试覆盖

项目经过完整的自动化回归测试，62 项测试全部通过。

### 管理 API 测试

- 查询所有 Mock API ✓
- 查询单个 Mock API ✓
- 查询不存在的 API → 404 ✓
- 创建 Mock API → 201 ✓
- 创建重复 Method+Path → 409 ✓
- 创建无效 JSON ResponseBody → 400 ✓
- 空 ResponseBody 拒绝保存 ✓
- 更新 Mock API ✓
- 更新不存在 API → 404 ✓
- 删除 Mock API → 204 ✓
- 删除不存在 API → 404 ✓

### 动态 Mock 测试

- GET /mock/users → 200 + 正确 JSON ✓
- GET /mock/products → 200 ✓
- POST /mock/login → 200 + token ✓
- 不存在接口 → 404 `${"code":404,"message":"Mock API not found"}` ✓
- 不同 Method 相同 Path 独立配置 ✓
- 中文路径正常处理 ✓
- 超长路径（200 字符）正常处理 ✓
- 配置 Status Code 500 正确返回 ✓
- 大响应体（100KB+）正常返回 ✓

### Dashboard 测试

- 返回 200 ✓
- 包含 API 总数、启用数、请求量 ✓
- 包含平均响应时间 ✓
- 包含近 7 天每日请求趋势数据 ✓
- 包含最近请求日志列表 ✓

### 日志测试

- 分页查询 ✓
- Method 筛选 ✓
- Status Code 筛选 ✓
- 清空日志 ✓
- pageSize 上限钳制（max 100） ✓

---

## 项目截图

> *Dashboard 仪表盘展示 Mock API 统计、请求趋势图和最近请求日志。*
>
> *Mock APIs 页面展示所有 Mock 接口列表，支持创建、编辑和删除。*
>
> *API Tester 页面提供请求发送和响应查看功能。*
>
> *Logs 页面展示请求历史记录，支持筛选和分页。*

---

## 设计文档

- [`CODE_REVIEW.md`](CODE_REVIEW.md) — 两轮 Code Review 完整记录（含审查标准与修复日志）

---

## License

MIT