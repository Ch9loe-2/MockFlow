# MockFlow — 面试问答资产

> 本项目是面向全栈 .NET + Vue 开发者岗位的 portfolio 项目。
> 下面的问题覆盖了面试中最高频的考察方向。

---

## 目录

1. [项目概览与动机](#1-项目概览与动机)
2. [架构决策](#2-架构决策)
3. [技术难点与解决方案](#3-技术难点与解决方案)
4. [Code Review 与质量保障](#4-code-review-与质量保障)
5. [安全考量](#5-安全考量)
6. [性能优化](#6-性能优化)
7. [扩展性设计](#7-扩展性设计)
8. [面试加分题](#8-面试加分题)

---

## 1. 项目概览与动机

### Q: 为什么做这个项目？解决了什么问题？

**A:** MockFlow 解决的是前后端分离开发中的典型阻塞问题：前端依赖后端接口，但后端接口可能迟迟未就绪。通常有几种方案：

- **手写 JSON 文件 + 前端硬编码**：不可维护，测试完要删改
- **Postman Mock Server**：需要额外配置，不能在团队中共享
- **YApi / Swagger**：太重，小团队不想搭

MockFlow 的定位是**轻量、零配置、可视化**。起一个 .NET 进程 + 一个 Vite 进程就有完整的管理后台，CRUD 页面 + 动态 Mock 响应 + 请求追踪全有了。

### Q: 对比同类竞品，你的差异化在哪？

| 特性 | MockFlow | Postman Mock | YApi |
|------|---------|-------------|------|
| 启动成本 | `dotnet run` + `npm run dev` | 需要登录 + 配置 | 需要部署 Node 服务 + MongoDB |
| Mock 管理 | Web UI + REST API | Web UI | Web UI |
| 请求追踪 | 内置（Dashboard + 日志分页） | 需要 Postman 控制台 | 内置 |
| API Tester | 内嵌 | Postman 本身 | 内嵌 |
| 代码量 | 后端 6 文件 / 前端 6 文件 | N/A | 重量级 |
| License | MIT | 商业 | Apache 2.0 |

---

## 2. 架构决策

### Q: 为什么用 Middleware 实现 Mock 路由，而不是 Controller？

**A:** 这个决策是项目的核心架构点。Mock 接口的特点是**动态增删**——用户在 UI 里创建一条记录，立即就应该可以访问。如果用 Controller：

- 需要为每个 Mock 接口写一个 Action（不可能，因为数量未知）
- 或者用某种通配路由 + 反射动态调用（过设计）

而 ASP.NET Core Middleware 天然适合这种场景：

```csharp
// 中间件在 Pipeline 中拦截 /mock/* 请求
// 查数据库 → 获取配置 → 写响应 → 记日志
if (!path.StartsWith("/mock/")) {
    await _next(context);  // 不处理，放行到 Controller
    return;
}
var mockApi = await mockApiService.FindMockIgnoreEnabledAsync(method, path);
if (mockApi == null) { /* 404 */ }
if (!mockApi.IsEnabled) { /* 403 */ }
// 写配置的响应体
```

**面试时这么说：** "我用 ASP.NET Core Middleware 做动态路由而不是 Controller，因为 Mock 接口的集合是运行时动态变化的。Middleware 可以在请求管道中按需拦截，不需要预先声明路由。这是一个『运行时路由』的典型应用场景。"

### Q: 为什么前后端分离？为什么用 Vite Proxy 而不是直接 CORS？

**A:** 前后端分离是团队协作的标准模式。Vite Dev Proxy 的作用是在开发阶段避免跨域问题：

- 前端请求 `/api/mock-apis` → Vite Proxy → `http://localhost:5001/api/mock-apis`
- 浏览器只看到同域请求，不需要处理 `OPTIONS` 预检请求
- 生产部署时用 nginx 统一反向代理，开发/生产环境一致

### Q: 为什么用 SQLite 而不是 PostgreSQL / SQL Server？

**A:** 这个项目的定位是**本地开发工具**，SQLite 是合理选择：

- 零配置：不需要安装数据库服务
- 文件级存储：`mockflow.db` 一个文件，可以随项目携带
- EF Core 完全兼容：CRUD + 索引 + 唯一约束全部支持
- 缺点：并发写能力弱，但对于个人开发工具不是问题
- 升级路径：EF Core 的抽象层使切换 PostgreSQL 只需要改 connection string + NuGet 包

### Q: DTO 层为什么用基类继承而不是接口？

**A:** `CreateMockApiDto` 和 `UpdateMockApiDto` 有完全相同的字段定义。最初是重复声明（6 个属性写了两次），第二轮 Code Review 发现这个问题后，提取了抽象基类 `MockApiBaseDto`：

```csharp
public abstract class MockApiBaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    // ... 其余属性
}

public class CreateMockApiDto : MockApiBaseDto { }
public class UpdateMockApiDto : MockApiBaseDto { }
```

好处：修改字段定义时只改一处即可。但也要注意，如果将来 Create 和 Update 有不同的验证逻辑（如 Update 允许部分更新），继承模式可能需要调整。

---

## 3. 技术难点与解决方案

### Q: 开发中遇到的最难的问题是什么？怎么解决的？

**A:** 两个最有价值的问题：

**① SQLite + EF Core 的 GroupBy 翻译限制**

Dashboard 需要展示过去 7 天每天的请求量。EF Core 生成的 SQL 里用了 `GroupBy(l.RequestedAt.Date)` 加上 `.Select(g.Key.ToString("MM-dd"))`。结果：

- SQL Server 可以翻译
- SQLite **不能翻译**，运行时抛 `InvalidOperationException`

解决方案：先把日期数据拉到内存，再用 LINQ to Objects 分组：

```csharp
// 先查原始日期（EF Core 可以翻译 Select）
var dates = await _context.RequestLogs
    .Where(l => l.RequestedAt >= sevenDaysAgo)
    .Select(l => l.RequestedAt.Date)
    .ToListAsync();

// 内存分组（LINQ to Objects）
var dailyRequests = dates
    .GroupBy(d => d)
    .Select(g => new DailyRequestCountDto {
        Date = g.Key.ToString("MM-dd"),
        Count = g.Count()
    })
    .ToList();
```

这种 "split query" 模式在生产项目中也很常见——不要让 ORM 做它不擅长的事情。

**② Middleware 中响应时间测量**

第一版把 `Stopwatch.Stop()` 放在了 `WriteAsync` 之前，导致测量的只是数据库查询时间，不包含序列化和网络传输时间。

```csharp
// 错误的顺序
sw.Start();
var result = await db.FindAsync(...);
context.Response.WriteAsync(result);
sw.Stop();  // ❌ WriteAsync 是异步的！此时还没有写完
```

修正后：

```csharp
sw.Start();
// ... 查数据库
await context.Response.WriteAsync(responseBody);
sw.Stop();  // ✅ 确保响应完全写出后才停止计时
logService.LogRequestAsync(..., sw.ElapsedMilliseconds);
```

### Q: Method + Path 联合唯一约束是怎么实现的？

**A:** 两层保证：

1. **数据库层**：EF Core 的 `HasIndex(e => new { e.Method, e.Path }).IsUnique()`
2. **应用层**：POST 和 PUT 操作用 try/catch 捕获 `UNIQUE constraint failed` 异常，返回 HTTP 409

两层都必要：数据库层保证数据一致性（并发下），应用层保证友好的错误提示。

---

## 4. Code Review 与质量保障

### Q: 你们的 Code Review 流程是怎样的？

**A:** MockFlow 经历了**两轮深度 Code Review**，每轮有明确的审查标准：

**第一轮（功能完整性）：**
- 所有组件是否覆盖 loading / empty / error 状态
- CRUD 操作是否正确处理边界情况（不存在 / 重复 / 无效输入）
- 前端 JSON 校验 + 后端独立校验
- 中间件正确记录所有 Mock 请求
- 测试覆盖 62 个 case

**第二轮（工程质量）：**
- 代码中的 dead code（未使用的变量、方法）
- 前端潜在的运行时错误（undefined 变量引用）
- 功能缺陷（4xx 筛选器精确匹配 vs 范围匹配）
- 安全加固（后端缺乏输入验证）
- 代码重复（DTO 重复声明）
- 生产/开发环境区分（ExceptionMiddleware）
- CORS 安全备注

### Q: 测试策略是什么？

**A:** 使用 Node.js http 模块编写自动化回归测试，覆盖 5 个维度 62 个测试用例：

1. **管理 API**（11 项）：CRUD + 重复/无效输入/不存在
2. **动态 Mock**（11 项）：正常响应 + 404 + 403 + 不同 Method 独立 + 中文路径 + 大响应体
3. **Dashboard**（7 项）：统计数据正确性
4. **请求日志**（9 项）：分页 + 筛选 + 清空
5. **边界情况**（10 项）：中文/长路径/状态码 500/大响应体

没有用 xUnit 或 MSTest，因为测试需要后端运行且需要 HTTP 往返。Node.js 的 http 模块更轻量，而且不需要 .NET 测试运行器。

---

## 5. 安全考量

### Q: 这个项目有哪些安全风险？你怎么处理的？

**A:** 这个项目的定位是本地开发工具，安全风险有限。但仍然做了以下处理：

| 风险 | 处理方式 |
|------|---------|
| 跨域（CORS） | `AllowAnyOrigin`（开发环境），README 标明生产环境需收紧 |
| 异常信息泄露 | `ExceptionMiddleware` 注入 `IHostEnvironment`，开发环境返回 detail，生产环境只返回 "Internal server error" |
| 输入注入 | JSON Body 的 Name/Path 长度限制（Name 100 / Path 500），Method 白名单 |
| SQL 注入 | EF Core 参数化查询天然免疫 |
| 敏感数据 | Demo 数据中登录 token 和密码均为 Mock 值 |

进一步提升方向：添加 API Key 鉴权、HTTPS、请求速率限制。

---

## 6. 性能优化

### Q: 做了哪些性能优化？

**A:** 两轮优化：

**第一轮（上一轮审查发现）：**
1. **Mock 中间件减少 DB 查询**：从 2 次查询（查 enabled + 查 disabled）优化为 1 次查询（查全部，内存中判断 IsEnabled）
2. **响应时间准确测量**：Stopwatch 在 `WriteAsync` 之后停止（之前错误地在之前停止，测量不完整）

**第二轮（本轮审查发现）：**
1. **死代码清理**：删除不再调用的 `FindDisabledMockAsync` 方法，减少维护心智负担
2. **DTO 基类提取**：减少 ~50% 重复代码，避免修改时遗漏一处

---

## 7. 扩展性设计

### Q: 如果要加一个功能——比如响应延迟模拟——怎么加？

**A:** 修改 `MockEndpointMiddleware.cs` 的 InvokeAsync 方法，在写入响应前加延迟：

```csharp
// 在 MockApi 模型上加 DelayMs 字段
public int DelayMs { get; set; } = 0;

// Middleware 中
if (mockApi.DelayMs > 0)
    await Task.Delay(mockApi.DelayMs);
```

只需要改 2 个文件：Model（加字段） + Middleware（加延迟逻辑）。前端 UI 加一个 "Delay (ms)" 输入框。

这就是 Middleware 架构的好处——核心逻辑集中在一处，功能扩展的"冲击面"很小。

### Q: 如果要支持多用户/团队协作呢？

**A:** 这是更复杂的变更，需要：

1. **数据库**：加 Users/Teams 表，MockApis 加 CreatedBy 外键
2. **鉴权**：ASP.NET Core Identity + JWT Bearer
3. **前端**：登录页 + Token 存储
4. **数据库切换**：SQLite → PostgreSQL（并发写需求）

属于"重构力度大但架构风险小"的变更。EF Core 抽象 + Service 层隔离使得业务逻辑修改量不大。

---

## 8. 面试加分题

### Q: 说说你的开发流程

**A:** 这个项目我采用"三阶段交付"模式：

**阶段一：功能开发（1 天）**
- 后端：EF Core 模型 → Service 层 → Controller → Middleware
- 前端：Vue 路由 → 各页面组件 → Axios 调用
- 基础验证：手动 curl 测试

**阶段二：Git 同步 + 文档**
- 9 个语义化 commit
- README 架构图 + API 文档
- LICENSE 文件

**阶段三：Code Review（两轮）**
- 第一轮：功能完整性审查 + 62 项自动化测试
- 第二轮：架构/安全/性能/可维护性审查 + 面试文档
- 每次修复后回归测试

这个流程体现了我对**工程化交付**的重视——不只是代码能跑，而是可维护、可展示、可以用来面试。

### Q: 如果 MockFlow 要上生产环境，你会做什么？

**A:** 按优先级：

1. **数据库**：SQLite → PostgreSQL（用 Docker Compose 编排）
2. **CORS**：`AllowAnyOrigin` → 白名单域名
3. **鉴权**：加 JWT / API Key
4. **部署**：Dockerfile 多阶段构建 + nginx 反向代理
5. **HTTPS**：Let's Encrypt
6. **日志**：ILogger → 结构化日志（Serilog）+ 文件滚动
7. **监控**：Health Check endpoint + Prometheus metrics
8. **CI/CD**：GitHub Actions 自动测试 + 构建 + 部署

这不是猜需求，而是每个 .NET 后端项目上生产的基本 checklist。

---

## 项目亮点总结（面试用）

1. **ASP.NET Core Middleware 做动态路由** — 不同于常规 Controller 模式，展示了对 ASP.NET Core Pipeline 的深入理解
2. **全栈独立交付** — 从数据库设计到前端 Vue 组件到自动化测试，一人全链路完成
3. **Code Review 驱动的质量保障** — 两轮系统审查 + 62 项回归测试，发现并修复了 DTO 重复、未定义变量、后端验证缺失等生产级问题
4. **工程化意识** — 语义化 Git 提交、README 架构图、测试覆盖报告、面试资产文档
5. **解决问题的深度** — SQLite GroupBy 限制、Middleware 响应时间测量不准、前后端双重验证等真正踩过的坑