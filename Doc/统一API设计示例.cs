using System.Net;
using Pek.Webs.Clients;

namespace Pek.Common.Examples;

/// <summary>统一 API 设计 - 所有方法都返回 HttpResponse</summary>
public class UnifiedApiDesign
{
    /// <summary>
    /// 统一方式 1：字符串响应 + 状态码
    /// </summary>
    public static async Task StringWithStatusCode()
    {
        var client = new WebClient();
        
        // ✅ 统一返回 HttpResponse<String>
        var response = await client.Get("https://api.example.com/text")
            .GetResponseAsync();
        
        // 可以直接判断状态码
        if (response.StatusCode == HttpStatusCode.OK)
            Console.WriteLine(response.Data);
        else
            Console.WriteLine($"错误: {response.StatusCode}");
    }

    /// <summary>
    /// 统一方式 2：JSON 响应 + 状态码
    /// </summary>
    public static async Task JsonWithStatusCode()
    {
        var client = new WebClient<User>();
        
        // ✅ 统一返回 HttpResponse<User>
        var response = await client.Get("https://api.example.com/user/1")
            .GetResponseAsync();
        
        // 同样可以判断状态码
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                Console.WriteLine($"用户: {response.Data?.Name}");
                break;
            case HttpStatusCode.NotFound:
                Console.WriteLine("用户不存在");
                break;
        }
    }

    /// <summary>
    /// 统一方式 3：字符串响应转 JSON（链式调用）
    /// </summary>
    public static async Task StringToJson()
    {
        var client = new WebClient();
        
        // ✅ 先拿字符串响应，再转 JSON
        var response = await client.Get("https://api.example.com/data")
            .GetResponseAsync();
        
        // 使用扩展方法转换
        var jsonResponse = response.AsJson<ApiResult>();
        
        if (jsonResponse.IsSuccess)
            Console.WriteLine(jsonResponse.Data?.Message);
    }

    /// <summary>
    /// 统一方式 4：使用便捷方法
    /// </summary>
    public static async Task UsingHelperMethods()
    {
        var client = new WebClient();
        
        var response = await client.Post("https://api.example.com/submit")
            .JsonContent(new { value = 123 })
            .GetResponseAsync();
        
        // ✅ 方法 1: EnsureSuccess（类似 HttpClient 的 EnsureSuccessStatusCode）
        try
        {
            response.EnsureSuccess(); // 失败时抛异常
            Console.WriteLine(response.Data);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"请求失败: {ex.Message}");
        }
        
        // ✅ 方法 2: GetDataOrDefault
        var data = response.GetDataOrDefault("默认值");
        Console.WriteLine(data);
        
        // ✅ 方法 3: 链式回调
        response
            .OnSuccess(data => Console.WriteLine($"成功: {data}"))
            .OnFailure((code, data) => Console.WriteLine($"失败 {code}: {data}"));
    }

    /// <summary>
    /// 统一方式 5：函数式风格
    /// </summary>
    public static async Task FunctionalStyle()
    {
        var client = new WebClient();
        
        var response = await client.Get("https://api.example.com/number")
            .GetResponseAsync();
        
        // ✅ 使用 Match 模式匹配
        response.Match(
            onSuccess: data => Console.WriteLine($"成功获取: {data}"),
            onFailure: (code, data) => Console.WriteLine($"失败 [{code}]: {data}")
        );
        
        // ✅ 使用 Map 转换类型
        var intResponse = response.Map(str => Int32.TryParse(str, out var n) ? n : 0);
        Console.WriteLine($"转换后: {intResponse.Data}");
    }

    /// <summary>
    /// 统一方式 6：复杂业务场景
    /// </summary>
    public static async Task ComplexScenario()
    {
        var client = new WebClient();
        
        var response = await client.Post("https://api.example.com/process")
            .JsonContent(new { orderId = 12345 })
            .Retry(3)
            .GetResponseAsync();
        
        // ✅ 统一的状态码处理
        var result = response.StatusCode switch
        {
            HttpStatusCode.OK => $"处理成功: {response.Data}",
            HttpStatusCode.BadRequest => $"参数错误: {response.Data}",
            HttpStatusCode.Unauthorized => "需要登录",
            HttpStatusCode.Forbidden => "无权限",
            HttpStatusCode.NotFound => "资源不存在",
            HttpStatusCode.TooManyRequests => "请求过于频繁",
            HttpStatusCode.InternalServerError => $"服务器错误: {response.Data}",
            _ => $"未知状态 {(Int32)response.StatusCode}: {response.Data}"
        };
        
        Console.WriteLine(result);
    }

    /// <summary>
    /// 对比：旧方式的问题
    /// </summary>
    public static async Task OldWayProblems()
    {
        var client = new WebClient();
        
        // ❌ 旧方式：拿不到状态码
        #pragma warning disable CS0618 // 类型或成员已过时
        var result = await client.Get("https://api.example.com/data")
            .ResultStringAsync();
        #pragma warning restore CS0618
        
        // ❓ 这个结果是成功还是失败？是 200 还是 400？
        // 无法判断！
        Console.WriteLine(result);
    }

    /// <summary>
    /// 新方式：完整信息
    /// </summary>
    public static async Task NewWayBenefits()
    {
        var client = new WebClient();
        
        // ✅ 新方式：完整信息
        var response = await client.Get("https://api.example.com/data")
            .GetResponseAsync();
        
        // ✅ 清晰知道所有信息
        Console.WriteLine($"状态码: {response.StatusCode}");
        Console.WriteLine($"是否成功: {response.IsSuccess}");
        Console.WriteLine($"内容类型: {response.ContentType}");
        Console.WriteLine($"响应数据: {response.Data}");
        
        // ✅ 可以做精细化处理
        if (response.IsSuccess)
        {
            ProcessSuccessData(response.Data);
        }
        else
        {
            LogError(response.StatusCode, response.Data);
        }
    }

    private static void ProcessSuccessData(String? data) { }
    private static void LogError(HttpStatusCode code, String? data) { }
}

/// <summary>示例类型</summary>
public class User
{
    public Int32 Id { get; set; }
    public String? Name { get; set; }
}

/// <summary>API 结果</summary>
public class ApiResult
{
    public Int32 Code { get; set; }
    public String? Message { get; set; }
    public Object? Data { get; set; }
}

/// <summary>
/// 设计哲学总结
/// </summary>
public class DesignPhilosophy
{
    /*
     * 统一设计原则：
     * 
     * 1. ✅ 所有 HTTP 方法都返回 HttpResponse<T>
     *    - 包含完整的状态码信息
     *    - 包含响应数据
     *    - 提供便捷的判断方法
     * 
     * 2. ✅ 不隐藏 HTTP 语义
     *    - 状态码是 HTTP 协议的一部分
     *    - 不应该被抽象掉
     *    - 调用者应该能够访问完整信息
     * 
     * 3. ✅ 向后兼容但引导升级
     *    - 旧方法标记为 Obsolete
     *    - 给出明确的迁移建议
     *    - 不强制立即修改
     * 
     * 4. ✅ 提供便捷方法
     *    - EnsureSuccess() - 确保成功
     *    - GetDataOrDefault() - 安全获取
     *    - OnSuccess/OnFailure - 链式回调
     *    - Match - 函数式模式匹配
     *    - Map - 类型转换
     * 
     * 5. ✅ 支持多种风格
     *    - 命令式：if/switch 判断
     *    - 函数式：Match/Map 链式调用
     *    - 异常式：EnsureSuccess() 抛异常
     */
}
