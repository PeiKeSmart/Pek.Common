# WebClient API 设计说明

## 概述

`WebClient` 提供统一、简洁的 HTTP 客户端 API，所有方法返回 `HttpResponse<T>`，包含完整的状态码和响应数据。

## 核心设计理念

### 统一返回类型

所有 HTTP 请求方法统一返回 `HttpResponse<T>`，包含：
- ✅ HTTP 状态码（`StatusCode`）
- ✅ 响应内容（`Data`）
- ✅ 成功判断（`IsSuccess`）
- ✅ 内容类型（`ContentType`）

这样开发者可以：
1. 明确知道请求的 HTTP 状态（200/400/500）
2. 根据状态码做精细化处理
3. 不依赖异常来判断 HTTP 错误状态

## 使用方式

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
- ✅异常处理

### 标准方式

使用标准的 try-catch 处理网络异常：

```csharp
try
{
    var response = await client.Get(url)
        .Retry(3)  // 可选：设置重试次数
        .GetResponseAsync();
    
    // 处理 HTTP 状态码
    if (response.StatusCode == HttpStatusCode.BadRequest)
        Console.WriteLine($"参数错误: {response.Data}");
    else if (response.IsSuccess)
        Console.WriteLine($"成功: {response.Data}");
}
catch (HttpRequestException ex)
{
    // 处理网络异常（DNS 失败、连接被拒绝等）
    Console.WriteLine($"网络错误: {ex.Message}");
}
catch (TaskCanceledException ex)
{
    // 处理超时
    Console.WriteLine($"请求超时: {ex.Message}");
}
```

### HTTP 状态码 vs 网络异常

**明确区分**：
- **HTTP 状态码**（400/401/500 等）→ `response.StatusCode`，不抛异常
- **网络异常**（超时、DNS 失败、连接拒绝）→ 抛异常，使用 try-catch

```csharp
var response = await client.Get(url).GetResponseAsync();

// ✅ HTTP 400 - 正常返回响应
if (response.StatusCode == HttpStatusCode.BadRequest)
    Console.WriteLine(response.Data);

// ❌ 网络超时 - 抛出 TaskCanceledException
// ❌ DNS 失败 - 抛出 HttpRequestException

### 状态码判断逻辑

```csharp
public Boolean IsSuccess => (Int32)StatusCode >= 200 && (Int32)StatusCode < 300;
```

### 重试机制

`GetResponseAsync()` 继承原有重试逻辑：

- 支持 `.Retry(count)` 设置重试次数
- 异常情况仍可通过 `WhenCatch` 处理（可选）

###API 概览

### 核心方法

```csharp
// 字符串响应
WebClient client = new();
HttpResponse<String> response = await client.Get(url).GetResponseAsync();

// JSON 对象响应（自动反序列化）
WebClient<User> client = new();
HttpResponse<User> response = await client.Get(url).GetResponseAsync();
```

### 流式反序列化优化

对于 JSON 响应，`WebClient<T>` 会自动使用流式反序列化，提升性能：
- 节省 **50% 内存分配**
- 性能提升 **20-40%**（大响应）
- 无需任何额外配置，自动启用

详见：[WebClient性能优化说明.md](WebClient性能优化说明.md) Task<HttpResponse<String>> GetResponseAsync();
}

public interface IHttpRequest<TResult>
{
    Task<TResult> ResultFromJsonAsync();
+   Task<HttpResponse<TResult>> GetResponseAsync();
}
```相关文档

- 📄 [HttpResponse使用示例.cs](HttpResponse使用示例.cs) - 完整使用示例
- 📄 [WebClient性能优化说明.md](WebClient性能优化说明.md) - 流式反序列化性能优化

---

**文档更新

**变更日期**: 2025-12-30  
**遵循规范**: PeiKeSmart Copilot 协作指令
