using Pek.Webs.Clients;

namespace Pek.Common.Examples;

/// <summary>ResultStringAsync 与 GetResponseAsync 对比示例</summary>
public class ApiMethodComparison
{
    /// <summary>
    /// 场景 1：简单场景，只关心内容
    /// 推荐：继续使用 ResultStringAsync
    /// </summary>
    public static async Task SimpleScenario()
    {
        var client = new WebClient();
        
        // ✅ 这样写完全没问题，继续用！
        var result = await client.Get("https://api.example.com/simple")
            .ResultStringAsync();
        
        Console.WriteLine(result);
    }

    /// <summary>
    /// 场景 2：需要区分状态码
    /// 推荐：使用 GetResponseAsync
    /// </summary>
    public static async Task NeedStatusCode()
    {
        var client = new WebClient();
        
        // ✅ 新方式：直接拿到状态码和内容
        var response = await client.Get("https://api.example.com/data")
            .GetResponseAsync();
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine("资源不存在");
            return;
        }
        
        if (response.IsSuccess)
        {
            Console.WriteLine(response.Data);
        }
    }

    /// <summary>
    /// 场景 3：已有代码使用 WhenCatch
    /// 建议：保持现状，无需改动
    /// </summary>
    public static async Task ExistingCodeWithCatch()
    {
        var client = new WebClient();
        
        // ✅ 这样写完全可以，不需要改！
        var result = await client.Get("https://api.example.com/old")
            .WhenCatch<Exception>(ex => "默认值")
            .ResultStringAsync();
        
        Console.WriteLine(result);
    }

    /// <summary>
    /// 场景 4：新业务，RESTful API
    /// 推荐：使用 GetResponseAsync
    /// </summary>
    public static async Task NewRestfulApi()
    {
        var client = new WebClient();
        
        // ✅ 新方式：标准 HTTP 语义
        var response = await client.Post("https://api.example.com/users")
            .JsonContent(new { name = "测试用户" })
            .GetResponseAsync();
        
        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Created:
                Console.WriteLine("创建成功");
                break;
            case System.Net.HttpStatusCode.BadRequest:
                Console.WriteLine($"参数错误: {response.Data}");
                break;
            case System.Net.HttpStatusCode.Conflict:
                Console.WriteLine("用户已存在");
                break;
        }
    }

    /// <summary>
    /// 场景 5：需要同时处理成功和失败
    /// 对比：旧方式 vs 新方式
    /// </summary>
    public static async Task HandleBothSuccessAndFailure()
    {
        var client = new WebClient();
        var url = "https://api.example.com/process";
        
        // ❌ 旧方式：需要闭包捕获
        String? content = null;
        System.Net.HttpStatusCode? statusCode = null;
        
        await client.Post(url)
            .OnSuccess((c, s) => { content = c; statusCode = s; })
            .OnFail((c, s) => { content = c; statusCode = s; })
            .ResultStringAsync();
        
        if (statusCode == System.Net.HttpStatusCode.OK)
            Console.WriteLine("成功");
        
        // ✅ 新方式：一行搞定
        var response = await client.Post(url).GetResponseAsync();
        if (response.IsSuccess)
            Console.WriteLine("成功");
    }

    /// <summary>
    /// 场景 6：异常处理 + 状态码判断
    /// 两种方式可以混用
    /// </summary>
    public static async Task ExceptionAndStatusCode()
    {
        var client = new WebClient();
        
        try
        {
            // ✅ GetResponseAsync 也支持 WhenCatch（但通常不需要了）
            var response = await client.Get("https://api.example.com/data")
                .Retry(3) // 重试仍然有效
                .GetResponseAsync();
            
            // 根据状态码处理
            if (response.IsSuccess)
            {
                Console.WriteLine(response.Data);
            }
            else
            {
                Console.WriteLine($"HTTP 错误: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // 网络异常（超时、DNS 失败等）
            Console.WriteLine($"网络异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 场景 7：泛型 JSON 反序列化
    /// 两种方式都可以
    /// </summary>
    public static async Task JsonDeserialization()
    {
        // ✅ 旧方式：继续可用
        var client1 = new WebClient<UserInfo>();
        var user = await client1.Get("https://api.example.com/user/1")
            .ResultFromJsonAsync();
        
        // ✅ 新方式：能拿到状态码
        var client2 = new WebClient<UserInfo>();
        var response = await client2.Get("https://api.example.com/user/1")
            .GetResponseAsync();
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            Console.WriteLine("用户不存在");
        }
        else if (response.IsSuccess)
        {
            Console.WriteLine($"用户名: {response.Data?.Name}");
        }
    }
}

/// <summary>示例用户类</summary>
public class UserInfo
{
    public Int32 Id { get; set; }
    public String? Name { get; set; }
}

/// <summary>
/// 总结：什么时候该用哪个？
/// </summary>
public class UsageGuidelines
{
    /*
     * ResultStringAsync / ResultFromJsonAsync：
     * ✅ 简单场景，只关心内容
     * ✅ 已有代码，懒得改
     * ✅ 不需要区分状态码
     * ✅ 使用 WhenCatch 处理网络异常
     * 
     * GetResponseAsync：
     * ✅ 需要判断 HTTP 状态码（400/401/404/500 等）
     * ✅ RESTful API 调用
     * ✅ 需要同时获取状态码和内容
     * ✅ 新业务代码
     * ✅ 想要更清晰的代码语义
     * 
     * 关键：
     * - 两者可以共存，不冲突
     * - 没有强制迁移要求
     * - GetResponseAsync 是可选的增强功能
     * - 根据实际需求选择
     */
}
