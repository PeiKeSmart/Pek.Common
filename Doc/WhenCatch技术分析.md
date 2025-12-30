# WhenCatch 为什么拿不到数据 - 技术分析

## 问题根源

在原有实现中，**`WhenCatch` 实际上并不是用来处理 HTTP 错误状态码（如 400、500）的**，而是用来处理**网络异常**（如连接超时、DNS 解析失败等）。

## 技术原理

### 1. HttpClient.SendAsync 的行为

```csharp
var response = await client.SendAsync(request);
```

**关键点**：`SendAsync` 在收到 4xx/5xx 状态码时**不会抛出异常**，它会正常返回 `HttpResponseMessage` 对象。

- ✅ 200 OK → 正常返回，`response.IsSuccessStatusCode = true`
- ✅ 400 Bad Request → 正常返回，`response.IsSuccessStatusCode = false`
- ✅ 500 Internal Server Error → 正常返回，`response.IsSuccessStatusCode = false`
- ❌ 网络超时 → 抛出 `TaskCanceledException`
- ❌ DNS 解析失败 → 抛出 `HttpRequestException`
- ❌ 连接被拒绝 → 抛出 `HttpRequestException`

### 2. 原有代码流程

```csharp
// HttpRequestBase.cs
protected async Task<String> ResultAsync()
{
    SendBefore();
    var response = await SendAsync().ConfigureAwait(false); // ← 4xx/5xx 不会异常
    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    SendAfter(result, response); // ← 会调用 FailHandler
    return result; // ← 正常返回内容（包括错误响应的 body）
}
```

```csharp
// HttpRequestBase.cs
protected virtual void SendAfter(String result, HttpResponseMessage response)
{
    var contentType = HttpRequestBase<TRequest>.GetContentType(response);
    if (response.IsSuccessStatusCode) // ← 4xx/5xx 时为 false
    {
        SuccessHandler(result, response.StatusCode, contentType);
        return;
    }
    FailHandler(result, response.StatusCode, contentType); // ← 会执行这里
}

protected virtual void FailHandler(String result, HttpStatusCode statusCode, String? contentType)
{
    _failAction?.Invoke(result); // ← 调用 OnFail 回调
    _failStatusCodeAction?.Invoke(result, statusCode);
    // 注意：这里只是回调，没有抛异常！
}
```

### 3. WhenCatch 的真实用途

```csharp
// HttpRequest.cs
public async Task<String> ResultStringAsync()
{
    var attempt = 0;
    while (true)
    {
        try
        {
            var result = await ResultAsync().ConfigureAwait(false); // ← 4xx/5xx 不会进 catch
            return result; // ← 直接返回（包括错误响应体）
        }
        catch (Exception ex) // ← 只捕获网络异常，不捕获 HTTP 状态码
        {
            if (++attempt > _retryCount)
            {
                if (_exceptionHandler != null)
                {
                    return _exceptionHandler.Invoke(ex); // ← WhenCatch 在这里生效
                }
                throw; // ← 没有 WhenCatch 时重新抛出
            }
        }
    }
}
```

## 为什么"拿不到数据"？

实际上 **`ResultAsync()` 会正常返回 4xx/5xx 的响应体**，但问题在于：

### 场景 1：使用 OnFail 回调

```csharp
var result = await client.Get(url)
    .OnFail((content, statusCode) => {
        Console.WriteLine($"失败：{statusCode}");
        // ← content 是有值的！
    })
    .ResultStringAsync();

// ← result 也是有值的（返回服务器的错误响应）
```

**这种情况下其实能拿到数据**，只是可能没有区分状态码。

### 场景 2：误用 WhenCatch

```csharp
var result = await client.Get(url)
    .WhenCatch<Exception>(ex => "发生错误") // ← 4xx/5xx 不会触发！
    .ResultStringAsync();

// ← 如果是 400，WhenCatch 不会执行，result 是服务器返回的原始内容
// ← 如果是网络超时，WhenCatch 才会执行
```

**误解**：开发者可能以为 `WhenCatch` 会处理 HTTP 错误状态，但实际上它只处理异常。

### 场景 3：某些服务器行为

某些服务器在返回 4xx/5xx 时可能：
- 响应体为空
- 响应头 `Content-Length: 0`
- 返回的是 HTML 错误页面而不是 JSON

这会导致 **数据看起来是空的或无效的**。

## 真相总结

| 情况 | `ResultAsync()` 行为 | `WhenCatch` 是否触发 | 数据是否可用 |
| --- | --- | --- | --- |
| 200 OK + 有数据 | 正常返回 | ❌ 否 | ✅ 是 |
| 400 Bad Request + 有 body | 正常返回 | ❌ 否 | ✅ 是（但需要手动检查状态码） |
| 500 Server Error + 有 body | 正常返回 | ❌ 否 | ✅ 是（但需要手动检查状态码） |
| 404 Not Found + 空 body | 正常返回空字符串 | ❌ 否 | ⚠️ 响应体本身就是空 |
| 网络超时 | 抛出异常 | ✅ 是 | ❌ 否（除非 WhenCatch 返回默认值） |
| DNS 解析失败 | 抛出异常 | ✅ 是 | ❌ 否 |

## 为什么需要新的 GetResponseAsync？

原有设计的问题：
1. ❌ **无法区分状态码**：`ResultStringAsync()` 只返回字符串，不知道是 200 还是 400
2. ❌ **语义混乱**：`WhenCatch` 名字暗示处理"错误"，但实际只处理"异常"
3. ❌ **需要额外回调**：要通过 `OnFail` 获取状态码，但拿不到返回值

新设计的优势：
1. ✅ **一次性获取所有信息**：`GetResponseAsync()` 返回 `{ StatusCode, Data, IsSuccess }`
2. ✅ **语义清晰**：不再混淆"异常"和"HTTP 错误状态"
3. ✅ **简化代码**：不需要多个回调，直接通过状态码判断

## 对比示例

### 原有方式（有缺陷）

```csharp
String? content = null;
HttpStatusCode? code = null;

var result = await client.Post(url)
    .OnSuccess((c, s) => { content = c; code = s; }) // ← 需要闭包捕获
    .OnFail((c, s) => { content = c; code = s; })    // ← 重复代码
    .WhenCatch<Exception>(ex => "网络异常")         // ← 只处理异常
    .ResultStringAsync();

// 无法直接判断 code 是否为 400
if (code == HttpStatusCode.BadRequest)
{
    // 处理参数错误
}
```

### 新方式（清晰直观）

```csharp
var response = await client.Post(url).GetResponseAsync();

switch (response.StatusCode)
{
    case HttpStatusCode.OK:
        Console.WriteLine(response.Data);
        break;
    case HttpStatusCode.BadRequest:
        Console.WriteLine($"参数错误: {response.Data}");
        break;
    case HttpStatusCode.Unauthorized:
        // 重新登录
        break;
}
```

## 结论

**"拿不到数据"的真相**：
- 数据其实一直都在 `ResultAsync()` 的返回值中
- 问题是**无法知道状态码**，也**无法优雅地处理不同状态**
- `WhenCatch` 只处理网络异常，不处理 HTTP 状态码

**新方案的价值**：
- 让 HTTP 状态码和响应内容同时可见
- 符合 RESTful API 的标准语义
- 不再混淆"异常"和"错误状态"

---

**补充说明**：如果之前确实遇到"拿不到数据"的情况，可能原因是：
1. 服务器返回的 4xx/5xx 响应体本身就是空的
2. 响应是 HTML 而不是预期的 JSON（需要解析处理）
3. 代码中有其他地方判断了 `IsSuccessStatusCode` 并提前返回空值
4. 使用了 `EnsureSuccessStatusCode()`（但在当前代码中没有找到）

如需进一步分析具体场景，请提供实际调用代码示例。
