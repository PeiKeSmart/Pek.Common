using System.Net;
using System.Text.Json;

using NewLife.Log;
using NewLife.Serialization;

using Pek.Helpers;

namespace Pek.Webs.Clients;

/// <summary>
/// Http请求
/// </summary>
public class HttpRequest : HttpRequestBase<IHttpRequest>, IHttpRequest
{
    /// <summary>
    /// 执行成功的回调函数
    /// </summary>
    private Action<String>? _successAction;

    /// <summary>
    /// 执行成功的回调函数
    /// </summary>
    private Action<String, HttpStatusCode>? _successStatusCodeAction;

    /// <summary>
    /// 初始化一个<see cref="HttpRequest"/>类型的实例
    /// </summary>
    /// <param name="httpMethod">Http请求方法</param>
    /// <param name="url">请求地址</param>
    public HttpRequest(HttpMethod httpMethod, String url) : base(httpMethod, url)
    {
    }

    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，参数为响应结果</param>
    public IHttpRequest OnSuccess(Action<String> action)
    {
        _successAction = action;
        return this;
    }

    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，第一个参数为响应结果，第二个参数为状态码</param>
    public IHttpRequest OnSuccess(Action<String, HttpStatusCode> action)
    {
        _successStatusCodeAction = action;
        return this;
    }

    /// <summary>
    /// 成功处理操作
    /// </summary>
    /// <param name="result">结果</param>
    /// <param name="statusCode">状态码</param>
    /// <param name="contentType">内容类型</param>
    protected override void SuccessHandler(String result, HttpStatusCode statusCode, String? contentType)
    {
        _successAction?.Invoke(result);
        _successStatusCodeAction?.Invoke(result, statusCode);
    }

    /// <summary>
    /// 获取完整 HTTP 响应（包含状态码与内容）
    /// </summary>
    public Task<HttpResponse<String>> GetResponseAsync()
    {
        return ExecuteWithRetryAsync(ResultWithResponseAsync);
    }
}

/// <summary>
/// Http请求
/// </summary>
/// <typeparam name="TResult">结果类型</typeparam>
public class HttpRequest<TResult> : HttpRequestBase<IHttpRequest<TResult>>, IHttpRequest<TResult>
    where TResult : class
{
    /// <summary>
    /// 执行成功的回调函数
    /// </summary>
    private Action<TResult?>? _successAction;

    /// <summary>
    /// 执行成功的回调函数
    /// </summary>
    private Action<TResult?, HttpStatusCode>? _successStatusCodeAction;

    /// <summary>
    /// 执行成功的转换函数
    /// </summary>
    private Func<String, TResult>? _convertAction;

    /// <summary>
    /// 初始化一个<see cref="HttpRequest{TResult}"/>类型的实例
    /// </summary>
    /// <param name="httpMethod">Http请求方法</param>
    /// <param name="url">请求地址</param>
    public HttpRequest(HttpMethod httpMethod, String url) : base(httpMethod, url)
    {
    }

    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，参数为响应结果</param>
    /// <param name="convertAction">将结果字符串转换为指定类型，当默认转换实现无法转换时使用</param>
    public IHttpRequest<TResult> OnSuccess(Action<TResult?> action, Func<String, TResult>? convertAction = null)
    {
        _successAction = action;
        _convertAction = convertAction;
        return this;
    }

    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，第一个参数为响应结果，第二个参数为状态码</param>
    /// <param name="convertAction">将结果字符串转换为指定类型，当默认转换实现无法转换时使用</param>
    public IHttpRequest<TResult> OnSuccess(Action<TResult?, HttpStatusCode> action, Func<String, TResult>? convertAction = null)
    {
        _successStatusCodeAction = action;
        _convertAction = convertAction;
        return this;
    }

    /// <summary>
    /// 成功处理操作
    /// </summary>
    /// <param name="result">结果</param>
    /// <param name="statusCode">状态码</param>
    /// <param name="contentType">内容类型</param>
    protected override void SuccessHandler(String result, HttpStatusCode statusCode, String? contentType)
    {
        var successResult = ConvertTo(result, contentType);
        _successAction?.Invoke(successResult);
        _successStatusCodeAction?.Invoke(successResult, statusCode);
    }

    /// <summary>
    /// 将结果字符串转换为指定类型
    /// </summary>
    /// <param name="result">结果</param>
    /// <param name="contentType">内容类型</param>
    private TResult? ConvertTo(String result, String? contentType)
    {
        if (typeof(TResult) == typeof(String))
            return Conv.CTo<TResult>(result);
        if (_convertAction != null)
            return _convertAction(result);
        if (contentType.SafeString().Equals("application/json", StringComparison.OrdinalIgnoreCase))
            return JsonHelper.ToJsonEntity<TResult>(result);
        return null;
    }

    /// <summary>
    /// 获取完整 HTTP 响应（包含状态码与内容）
    /// </summary>
    public Task<HttpResponse<TResult>> GetResponseAsync()
    {
        return ExecuteWithRetryAsync(async () =>
        {
            var (response, contentType) = await GetRawResponseAsync().ConfigureAwait(false);
            
            // 性能优化：JSON 直接从 Stream 反序列化，避免中间字符串分配
            if (contentType.SafeString().Equals("application/json", StringComparison.OrdinalIgnoreCase))
            {
#if NETCOREAPP
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
                var data = await JsonSerializer.DeserializeAsync<TResult>(stream).ConfigureAwait(false);
                
                // 注意：Stream 已被读取，无法再次读取用于 SendAfter
                // 为了保持回调兼容性，传递空字符串
                SendAfter(String.Empty, response);
                
                return new HttpResponse<TResult>(response.StatusCode, data, response);
            }
            
            // 非 JSON 或需要自定义转换：回退到字符串方式
            var rawResponse = await ResultWithResponseAsync().ConfigureAwait(false);
            var convertedData = ConvertTo(rawResponse.Data!, rawResponse.ContentType);
            return new HttpResponse<TResult>(rawResponse.StatusCode, convertedData, rawResponse.RawResponse!);
        });
    }
}