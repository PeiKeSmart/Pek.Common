using Pek.Webs.Clients;
using System.Net;

namespace Pek.Common.Examples;

/// <summary>WebClient 使用示例</summary>
public class HttpResponseUsageExample
{
    /// <summary>
    /// 基础用法：字符串响应 + 状态码
    /// </summary>
    public static async Task BasicStringResponse()
    {
        var client = new WebClient();
        var response = await client.Get("https://api.example.com/text").GetResponseAsync();
        
        if (response.IsSuccess)
            Console.WriteLine($"成功: {response.Data}");
        else
            Console.WriteLine($"失败 ({response.StatusCode}): {response.Data}");
    }

    /// <summary>
    /// JSON 对象响应（自动反序列化）
    /// </summary>
    public static async Task JsonObjectResponse()
    {
        var client = new WebClient<User>();
        var response = await client.Get("https://api.example.com/user/1").GetResponseAsync();
        
        if (response.IsSuccess)
        {
            Console.WriteLine($"用户: {response.Data?.Name}");
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Console.WriteLine("用户不存在");
        }
    }

    /// <summary>
    /// 精细化状态码处理
    /// </summary>
    public static async Task DetailedStatusCodeHandling()
    {
        var client = new WebClient();
        var response = await client.Post("https://api.example.com/submit")
            .JsonContent(new { value = 123 })
            .GetResponseAsync();
        
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
            case HttpStatusCode.Created:
                Console.WriteLine($"成功: {response.Data}");
                break;
            
            case HttpStatusCode.BadRequest:
                Console.WriteLine($"参数错误: {response.Data}");
                break;
            
            case HttpStatusCode.Unauthorized:
                Console.WriteLine("需要登录");
                break;
            
            case HttpStatusCode.Forbidden:
                Console.WriteLine("无权限");
                break;
            
            case HttpStatusCode.TooManyRequests:
                Console.WriteLine("请求过于频繁，请稍后重试");
                break;
            
            case HttpStatusCode.InternalServerError:
                Console.WriteLine($"服务器错误: {response.Data}");
                break;
            
            default:
                Console.WriteLine($"未知状态 {(Int32)response.StatusCode}: {response.Data}");
                break;
        }
    }

    /// <summary>
    /// 异常处理：网络异常 vs HTTP 状态码
    /// </summary>
    public static async Task ExceptionHandling()
    {
        var client = new WebClient();
        
        try
        {
            var response = await client.Get("https://api.example.com/data")
                .Retry(3)  // 重试 3 次
                .GetResponseAsync();
            
            // HTTP 4xx/5xx 不会抛异常，正常返回
            if (response.StatusCode == HttpStatusCode.BadRequest)
                Console.WriteLine($"参数错误: {response.Data}");
            else if (response.IsSuccess)
                Console.WriteLine(response.Data);
        }
        catch (HttpRequestException ex)
        {
            // 网络异常：DNS 失败、连接被拒绝等
            Console.WriteLine($"网络错误: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            // 超时
            Console.WriteLine($"请求超时: {ex.Message}");
        }
    }

    /// <summary>
    /// 使用便捷方法
    /// </summary>
    public static async Task UsingHelperMethods()
    {
        var client = new WebClient();
        var response = await client.Get("https://api.example.com/data").GetResponseAsync();
        
        // 方法 1: EnsureSuccess（类似 HttpClient.EnsureSuccessStatusCode）
        try
        {
            response.EnsureSuccess();
            Console.WriteLine(response.Data);
        }
        catch (HttpRequestException)
        {
            Console.WriteLine("请求失败");
        }
        
        // 方法 2: GetDataOrDefault
        var data = response.GetDataOrDefault("默认值");
        
        // 方法 3: 链式回调
        response
            .OnSuccess(d => Console.WriteLine($"成功: {d}"))
            .OnFailure((code, d) => Console.WriteLine($"失败 {code}: {d}"));
        
        // 方法 4: Match 模式匹配
        response.Match(
            onSuccess: d => Console.WriteLine($"成功: {d}"),
            onFailure: (code, d) => Console.WriteLine($"失败 {code}: {d}")
        );
    }

    /// <summary>
    /// 字符串转 JSON（手动转换）
    /// </summary>
    public static async Task StringToJson()
    {
        var client = new WebClient();
        var response = await client.Get("https://api.example.com/user").GetResponseAsync();
        
        // 使用扩展方法转 JSON
        var userResponse = response.AsJson<User>();
        
        if (userResponse.IsSuccess)
            Console.WriteLine($"用户: {userResponse.Data?.Name}");
    }

    /// <summary>
    /// 复杂业务场景：下单流程
    /// </summary>
    public static async Task ComplexBusinessScenario()
    {
        var client = new WebClient<OrderResult>();
        
        var response = await client.Post("https://api.example.com/orders")
            .JsonContent(new { productId = 123, quantity = 2 })
            .Retry(2)
            .GetResponseAsync();
        
        var result = response.StatusCode switch
        {
            HttpStatusCode.Created => $"订单创建成功: {response.Data?.OrderId}",
            HttpStatusCode.BadRequest => $"参数错误: {response.Data?.Message}",
            HttpStatusCode.PaymentRequired => "余额不足",
            HttpStatusCode.Conflict => "库存不足",
            HttpStatusCode.TooManyRequests => "下单过于频繁",
            _ => response.IsSuccess 
                ? $"成功: {response.Data?.OrderId}"
                : $"失败 {response.StatusCode}: {response.Data?.Message}"
        };
        
        Console.WriteLine(result);
    }
}

/// <summary>示例用户类</summary>
public class User
{
    public Int32 Id { get; set; }
    public String? Name { get; set; }
    public String? Email { get; set; }
}

/// <summary>订单结果</summary>
public class OrderResult
{
    public String? OrderId { get; set; }
    public String? Message { get; set; }
}
