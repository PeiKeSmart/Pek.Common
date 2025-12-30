using Pek.Webs.Clients;

namespace Pek.Common.Examples;

/// <summary>WebClient 使用 HttpResponse 包装器示例</summary>
public class HttpResponseUsageExample
{
    /// <summary>
    /// 使用 GetResponseAsync 获取包含状态码的完整响应
    /// </summary>
    public static async Task ExampleGetResponseAsync()
    {
        var client = new WebClient();
        
        // 发起请求并获取完整响应（包含状态码）
        var response = await client.Get("https://api.example.com/data")
            .GetResponseAsync();
        
        // 检查状态码
        if (response.IsSuccess)
        {
            Console.WriteLine($"成功: {response.Data}");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            Console.WriteLine($"请求错误 (400): {response.Data}");
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            Console.WriteLine($"未授权 (401): {response.Data}");
        }
        else
        {
            Console.WriteLine($"失败 ({(Int32)response.StatusCode}): {response.Data}");
        }
    }

    /// <summary>
    /// 泛型版本：获取反序列化后的对象及状态码
    /// </summary>
    public static async Task ExampleGetResponseAsyncTyped()
    {
        var client = new WebClient<ApiResult>();
        
        var response = await client.Post("https://api.example.com/submit")
            .JsonContent(new { name = "测试" })
            .GetResponseAsync();
        
        // 使用 IsSuccess 判断是否为 2xx 状态码
        if (response.IsSuccess)
        {
            Console.WriteLine($"成功: {response.Data?.Message}");
        }
        else
        {
            // 即使是 4xx/5xx 也能拿到响应体数据
            Console.WriteLine($"状态码: {response.StatusCode}");
            Console.WriteLine($"错误信息: {response.Data?.Message}");
        }
    }

    /// <summary>
    /// 对比旧方式：ResultStringAsync 仍然可用（向后兼容）
    /// </summary>
    public static async Task ExampleResultStringAsync()
    {
        var client = new WebClient();
        
        try
        {
            // 旧方式：只拿字符串内容，异常情况通过 WhenCatch 处理
            var result = await client.Get("https://api.example.com/data")
                .WhenCatch<Exception>(ex => "发生错误")
                .ResultStringAsync();
            
            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 新方式的优势：无需 WhenCatch，直接通过状态码判断
    /// </summary>
    public static async Task ExampleNewApproachAdvantage()
    {
        var client = new WebClient();
        
        // 不再需要 WhenCatch，所有响应（包括错误状态码）都能正常拿到数据
        var response = await client.Post("https://api.example.com/process")
            .JsonContent(new { value = 123 })
            .Retry(3) // 仍然支持重试
            .GetResponseAsync();
        
        // 根据状态码灵活处理
        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.OK:
                Console.WriteLine($"处理成功: {response.Data}");
                break;
            
            case System.Net.HttpStatusCode.BadRequest:
                Console.WriteLine($"参数错误: {response.Data}");
                break;
            
            case System.Net.HttpStatusCode.InternalServerError:
                Console.WriteLine($"服务器错误: {response.Data}");
                break;
            
            default:
                Console.WriteLine($"其他状态 ({response.StatusCode}): {response.Data}");
                break;
        }
    }
}

/// <summary>示例 API 返回类型</summary>
public class ApiResult
{
    public Int32 Code { get; set; }
    public String? Message { get; set; }
    public Object? Data { get; set; }
}
