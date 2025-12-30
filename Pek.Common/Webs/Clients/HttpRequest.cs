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
    /// 异常处理函数
    /// </summary>
    protected Func<Exception, String>? _exceptionHandler;

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
    /// 设置异常处理函数
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <param name="func">异常处理函数</param>
    public IHttpRequest WhenCatch<TException>(Func<TException, String> func) where TException : Exception
    {
        _exceptionHandler = ex => func((TException)ex);
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
    public async Task<HttpResponse<String>> GetResponseAsync()
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                var response = await ResultWithResponseAsync().ConfigureAwait(false);
                return response;
            }
            catch (Exception ex)
            {
                if (++attempt > _retryCount)
                {
                    XTrace.Log.Error("请求链接失败：{0} {1}", Url, ex);

                    if (_exceptionHandler != null)
                    {
                        XTrace.Log.Error($"请求在重试 {_retryCount} 次后失败", ex);
                        var errorData = _exceptionHandler.Invoke(ex);
                        return new HttpResponse<String>(System.Net.HttpStatusCode.InternalServerError, errorData);
                    }
                    throw;
                }
            }
        }
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
    /// 异常处理函数
    /// </summary>
    protected Func<Exception, TResult>? _exceptionHandler;

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
    /// 设置异常处理函数
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <param name="func">异常处理函数</param>
    public IHttpRequest<TResult> WhenCatch<TException>(Func<TException, TResult> func) where TException : Exception
    {
        _exceptionHandler = ex => func((TException)ex);
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
        if (contentType.SafeString().Equals("application/json", StringComparison.CurrentCultureIgnoreCase))
            return JsonHelper.ToJsonEntity<TResult>(result);
        return null;
    }

    /// <summary>
    /// 获取完整 HTTP 响应（包含状态码与内容）
    /// </summary>
    public async Task<HttpResponse<TResult>> GetResponseAsync()
    {
        var attempt = 0;
        while (true)
        {
            try
            {
                var rawResponse = await ResultWithResponseAsync().ConfigureAwait(false);
                var convertedData = ConvertTo(rawResponse.Data!, rawResponse.ContentType);
                return new HttpResponse<TResult>(rawResponse.StatusCode, convertedData, rawResponse.RawResponse!);
            }
            catch (Exception ex)
            {
                if (++attempt > _retryCount)
                {
                    XTrace.Log.Error("请求链接失败：{0} {1}", Url, ex);

                    if (_exceptionHandler != null)
                    {
                        XTrace.Log.Error($"请求在重试 {_retryCount} 次后失败", ex);
                        var errorData = _exceptionHandler.Invoke(ex);
                        return new HttpResponse<TResult>(System.Net.HttpStatusCode.InternalServerError, errorData);
                    }
                    throw;
                }
            }
        }
    }
}