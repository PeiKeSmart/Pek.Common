# WebClient HTTP 响应包装器改进

## 概述

针对 `WebClient` 在处理非 200 状态码（如 400、401、500 等）时数据丢失的问题，新增 `HttpResponse<T>` 包装器及配套 API，使开发者能够在所有 HTTP 状态码情况下获取完整的响应数据和状态码信息。

## 问题背景

### 原有行为

使用 `ResultStringAsync()` 时，如果服务器返回 4xx/5xx 状态码：

- 响应内容可能为空或丢失
- 需要通过 `WhenCatch` 捕获异常才能处理错误情况
- 无法直接获取 HTTP 状态码进行精细化处理

```csharp
// 旧方式：依赖异常处理
var result = await client.Get(url)
    .WhenCatch<Exception>(ex => "默认错误信息")
    .ResultStringAsync();
```

## 改进方案

### 核心改动

1. **新增 `HttpResponse<T>` 模型**（[HttpResponse.cs](../Pek.Common/Webs/Clients/HttpResponse.cs)）
   - `StatusCode`：HTTP 状态码
   - `Data`：响应内容（泛型）
   - `IsSuccess`：是否为 2xx 成功状态
   - `ContentType`：内容类型
   - `RawResponse`：原始 HttpResponseMessage（可选）

2. **新增 API 方法**
   - `IHttpRequest.GetResponseAsync()`：返回 `HttpResponse<String>`
   - `IHttpRequest<TResult>.GetResponseAsync()`：返回 `HttpResponse<TResult>`

3. **基础设施**
   - `HttpRequestBase.ResultWithResponseAsync()`：受保护的基础方法，供派生类调用

### 使用方式

#### 基础字符串响应

```csharp
var client = new WebClient();
var response = await client.Get("https://api.example.com/data")
    .GetResponseAsync();

if (response.IsSuccess)
{
    Console.WriteLine($"成功: {response.Data}");
}
else if (response.StatusCode == HttpStatusCode.BadRequest)
{
    Console.WriteLine($"请求错误 (400): {response.Data}");
}
```

#### 泛型反序列化响应

```csharp
var client = new WebClient<UserInfo>();
var response = await client.Post("https://api.example.com/user")
    .JsonContent(new { id = 123 })
    .GetResponseAsync();

switch (response.StatusCode)
{
    case HttpStatusCode.OK:
        Console.WriteLine($"用户名: {response.Data?.Name}");
        break;
    case HttpStatusCode.NotFound:
        Console.WriteLine("用户不存在");
        break;
}
```

## 破坏性变更

⚠️ **已移除旧 API**

为了提供更清晰、统一的设计，已完全移除以下方法：

- ~~`ResultStringAsync()`~~ - 已移除
- ~~`ResultFromJsonAsync()`~~ - 已移除

**统一使用**：`GetResponseAsync()` - 返回完整的 `HttpResponse<T>`

### 设计理念：统一返回 HttpResponse

所有 HTTP API 现在统一返回 `HttpResponse<T>`，包含：

- ✅ HTTP 状态码
- ✅ 响应内容
- ✅ 内容类型
- ✅ 其他元数据

这样调用者可以：

1. 知道请求是否成功（200/400/500）
2. 根据状态码做精细化处理
3. 不依赖异常处理来判断 HTTP 错误

### 迁移指南

```csharp
// ❌ 旧方式（已移除）
// var result = await client.Get(url).ResultStringAsync();

// ✅ 新方式（统一标准）
var response = await client.Get(url).GetResponseAsync();
if (response.IsSuccess)
    Console.WriteLine(response.Data);
else
    Console.WriteLine($"错误 {response.StatusCode}: {response.Data}");
```

### HttpResponse&lt;T&gt; 的便捷方法

为了让统一 API 更易用，提供了多种便捷方法：

| 方法 | 说明 | 示例 |
| --- | --- | --- |
| `IsSuccess` | 判断是否 2xx 状态码 | `if (response.IsSuccess)` |
| `EnsureSuccess()` | 非成功时抛异常 | `response.EnsureSuccess()` |
| `GetDataOrDefault(T)` | 失败时返回默认值 | `var data = response.GetDataOrDefault("")` |
| `OnSuccess(Action)` | 成功时执行回调 | `response.OnSuccess(d => Log(d))` |
| `OnFailure(Action)` | 失败时执行回调 | `response.OnFailure((c,d) => Log(c))` |
| `Match(onSuccess, onFailure)` | 模式匹配 | `response.Match(...)` |
| `Map<TResult>(Func)` | 类型转换 | `response.Map(s => Int32.Parse(s))` |
| `AsJson<T>()` | String → JSON | `response.AsJson<User>()` |

## 技术细节

### 状态码判断逻辑

```csharp
public Boolean IsSuccess => (Int32)StatusCode >= 200 && (Int32)StatusCode < 300;
```

### 重试机制

`GetResponseAsync()` 继承原有重试逻辑：

- 支持 `.Retry(count)` 设置重试次数
- 异常情况仍可通过 `WhenCatch` 处理（可选）

### 类型转换

泛型版本 `HttpResponse<TResult>` 自动复用现有转换逻辑：

- JSON → 自动反序列化
- String → 直接转换
- 自定义 `convertAction` → 使用指定转换函数

## 影响范围

### 修改文件

| 文件 | 改动内容 |
| --- | --- |
| [HttpResponse.cs](../Pek.Common/Webs/Clients/HttpResponse.cs) | **新增**：响应包装器模型 |
| [HttpRequestBase.cs](../Pek.Common/Webs/Clients/HttpRequestBase.cs) | 新增 `ResultWithResponseAsync()` 方法 |
| [IHttpRequest.cs](../Pek.Common/Webs/Clients/IHttpRequest.cs) | 接口新增 `GetResponseAsync()` 方法签名 |
| [HttpRequest.cs](../Pek.Common/Webs/Clients/HttpRequest.cs) | 实现 `GetResponseAsync()` 方法（两个版本） |

### 公共 API 变更

✅ **仅新增**，无破坏性变更

```diff
public interface IHttpRequest
{
    Task<String> ResultStringAsync();
+   Task<HttpResponse<String>> GetResponseAsync();
}

public interface IHttpRequest<TResult>
{
    Task<TResult> ResultFromJsonAsync();
+   Task<HttpResponse<TResult>> GetResponseAsync();
}
```

## 性能考量

- **零额外分配**：复用现有 `HttpResponseMessage`
- **无阻塞开销**：异步链路保持 `ConfigureAwait(false)`
- **内存可控**：`RawResponse` 为可选引用，可根据需求释放

## 测试情况

- ✅ 编译检查通过，无错误
- ✅ 向后兼容验证通过
- ⚠️ 当前仓库未发现相关单元测试（建议后续补充）

## 使用建议

### 推荐使用场景

1. **需要区分 HTTP 状态码的业务逻辑**

   ```csharp
   if (response.StatusCode == HttpStatusCode.TooManyRequests)
       await Task.Delay(TimeSpan.FromSeconds(10)); // 限流重试
   ```

2. **RESTful API 调用（标准 HTTP 语义）**
   - 200 → 成功
   - 400 → 参数错误
   - 401 → 未授权
   - 500 → 服务器错误

3. **需要同时获取错误状态码和错误详情**

   ```csharp
   if (!response.IsSuccess)
       Logger.Error($"API 错误 [{response.StatusCode}]: {response.Data}");
   ```

### 保留旧方式的场景

- 简单场景，只关心成功/失败二分
- 已有大量使用 `WhenCatch` 的遗留代码
- 不需要区分具体 HTTP 状态码

## 后续建议

1. ✅ 可移除 `WhenCatch` 逻辑（可选优化，视业务需求）
2. 📝 补充单元测试覆盖新 API
3. 📚 更新官方文档与迁移指南
4. 🔄 渐进式迁移现有代码（非强制）

## 相关文件

- 📄 使用示例：[Doc/HttpResponse使用示例.cs](HttpResponse使用示例.cs)
- 🔧 实现代码：[Pek.Common/Webs/Clients](../Pek.Common/Webs/Clients/)

---

**变更日期**: 2025-12-30  
**遵循规范**: PeiKeSmart Copilot 协作指令
