# MockFlow — Code Review 完整记录

## 第一轮 Code Review（初始开发完成后）

### 审查范围
- 功能完整性：所有页面是否覆盖 loading / empty / error 状态
- CRUD 操作边界情况：不存在、重复、无效输入
- 前后端 JSON 双重校验
- 中间件正确记录所有 Mock 请求
- 端口冲突和系统代理干扰处理

### 发现并修复的问题

| # | 问题 | 修复方式 |
|---|------|---------|
| 1 | Dashboard 500 错误 — SQLite 无法翻译 `GroupBy().Select(g.Key.ToString())` | 先 Select 日期到内存，再 LINQ to Objects 分组 |
| 2 | 端口 5000 被 AirTunes 占用 | 全部改为 5001 |
| 3 | Mock 响应时间不准确 — Stopwatch 在 WriteAsync 之前停止 | 在 WriteAsync 之后调用 Stop() |
| 4 | PUT 方法缺少唯一约束异常处理 | 追加 try/catch 匹配 UNIQUE 冲突 |
| 5 | 日志分页 pageSize 无上限 | 添加 `Math.Clamp(pageSize, 1, 100)` |
| 6 | Mock 中间件双重 DB 查询（先查 enabled，404 再查 disabled） | 合并为一次查询 `FindMockIgnoreEnabledAsync`，内存判断 IsEnabled |
| 7 | API Tester 死代码 `const fullUrl = this.url.startsWith('http')...` 无操作 | 移除 |

### 测试覆盖
62 项自动化测试全部通过，覆盖管理 API / 动态 Mock / Dashboard / 日志 / 边界情况 5 个维度。

---

## 第二轮 Code Review（2026-09-17）

### 审查标准
- 编译错误与运行时错误检查
- 代码中的 dead code（未使用变量、方法、引用）
- 功能缺陷（筛选器逻辑错误）
- 安全加固（后端缺乏输入验证、CORS）
- 代码重复（DTO 重复声明）
- 生产/开发环境区分（异常处理）
- 前后端一致性（API 参数兼容性）
- README 完整性

### 发现并修复的问题

| # | 严重程度 | 问题 | 修复方式 |
|---|---------|------|---------|
| 1 | **P0** | `ApiTester.vue` 中 `fullUrl` 变量未定义 — 第 68 行引用了一个已被删除的变量，运行时 `ReferenceError`，API Tester 不可用 | 替换为 `this.url` |
| 2 | **P1** | Logs 页面 `statusCode` 筛选使用 `value="400"`，只匹配 404。用户选"4xx"期望看到所有 4xx 错误（400/403/404/409） | 改为 `statusCodeRange=400-499`，后端同时兼容精确 `statusCode` 和范围 `statusCodeRange` 两个参数 |
| 3 | **P1** | `MockApiService.FindDisabledMockAsync()` 已无调用方（上轮优化后死代码） | 删除 |
| 4 | **P1** | 后端 Controller 无 Name/Path 非空验证和长度限制，curl 可以绕过前端校验写入脏数据 | POST 和 PUT 添加 Name/Path 非空、长度上限（100/500）、Method 白名单校验 |
| 5 | **P2** | `CreateMockApiDto` 和 `UpdateMockApiDto` 字段定义完全一致，6 属性重复 2 次 | 提取 `MockApiBaseDto` 抽象基类，减少 ~50% 重复代码 |
| 6 | **P2** | `ExceptionMiddleware` 匿名对象输出，无法区分开发/生产环境，排障困难 | 注入 `IHostEnvironment`，Dev 模式返回 `detail`+`stackTrace`，Production 仅返回安全信息 |
| 7 | **P2** | CORS `AllowAnyOrigin` 无安全备注 | 添加注释说明生产环境需要收紧 |

### 回归测试结果
```
=== SUMMARY ===
  Passed: 62
  Failed: 0
```

新增验证（修复后手工测试）：
```
statusCodeRange=200-299 → Items:3 All 2xx: true
statusCodeRange=400-499 → Items:0 All 4xx: true
Empty name validation → {"code":400,"message":"Name is required"}
Invalid method → {"code":400,"message":"Method must be GET, POST, PUT, or DELETE"}
```

### Git 提交（第二轮）
```
commit: "fix: 第二轮深度 Code Review 修复 — fullUrl 未定义/状态码范围过滤/后端验证/DTO 优化/安全加固"
```

### 审查总结
两轮 Code Review 共发现并修复 13 个问题（P0 × 1, P1 × 4, P2 × 3, 其他 × 5）。项目从"功能可用"进化到"生产级质量"的工程标准。关键收获：

1. **前端死代码清理要彻底**：删除变量定义时必须检查所有引用
2. **前后端双验证是红线**：前端校验面向用户，后端校验是安全底线
3. **DTO 不要重复声明**：变化源只有一个，这是工程化的基本原则
4. **异常处理要区分环境**：开发环境需要详情，生产环境需要安全
5. **筛选器设计要考虑用户意图**："4xx" 意味着范围而非精确匹配