using System.Net;
using System.Text.Json;

namespace Pek.Webs.Clients;

/// <summary>HTTP 响应包装器，包含状态码与响应内容</summary>
/// <typeparam name="T">响应数据类型</typeparam>
public class HttpResponse<T>
{
    /// <summary>HTTP 状态码</summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>响应内容</summary>
    public T? Data { get; set; }

    /// <summary>是否为成功状态码（2xx）</summary>
    public Boolean IsSuccess => (Int32)StatusCode >= 200 && (Int32)StatusCode < 300;

    /// <summary>内容类型</summary>
    public String? ContentType { get; set; }

    /// <summary>原始响应消息（可选保留）</summary>
    public HttpResponseMessage? RawResponse { get; set; }

    /// <summary>初始化一个<see cref="HttpResponse{T}"/>类型的实例</summary>
    public HttpResponse()
    {
    }

    /// <summary>初始化一个<see cref="HttpResponse{T}"/>类型的实例</summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <param name="data">响应数据</param>
    /// <param name="contentType">内容类型</param>
    public HttpResponse(HttpStatusCode statusCode, T? data, String? contentType = null)
    {
        StatusCode = statusCode;
        Data = data;
        ContentType = contentType;
    }

    /// <summary>初始化一个<see cref="HttpResponse{T}"/>类型的实例</summary>
    /// <param name="statusCode">HTTP 状态码</param>
    /// <param name="data">响应数据</param>
    /// <param name="response">原始响应消息</param>
    public HttpResponse(HttpStatusCode statusCode, T? data, HttpResponseMessage response)
    {
        StatusCode = statusCode;
        Data = data;
        RawResponse = response;
        ContentType = response.Content?.Headers?.ContentType?.MediaType;
    }

    /// <summary>确保成功状态码，否则抛出异常</summary>
    /// <returns>当前实例</returns>
    /// <exception cref="HttpRequestException">非成功状态码时抛出</exception>
    public HttpResponse<T> EnsureSuccess()
    {
        if (!IsSuccess)
            throw new HttpRequestException($"HTTP 请求失败，状态码: {(Int32)StatusCode} ({StatusCode})");
        return this;
    }

    /// <summary>如果成功则获取数据，否则返回默认值</summary>
    /// <param name="defaultValue">失败时的默认值</param>
    public T? GetDataOrDefault(T? defaultValue = default) => IsSuccess ? Data : defaultValue;

    /// <summary>转换为另一种类型的响应</summary>
    /// <typeparam name="TResult">目标类型</typeparam>
    /// <param name="converter">转换函数</param>
    public HttpResponse<TResult> Map<TResult>(Func<T?, TResult?> converter)
    {
        return new HttpResponse<TResult>(StatusCode, converter(Data), ContentType)
        {
            RawResponse = RawResponse
        };
    }
}

/// <summary>HttpResponse 扩展方法</summary>
public static class HttpResponseExtensions
{
    /// <summary>将字符串响应反序列化为 JSON 对象</summary>
    /// <typeparam name="TResult">目标类型</typeparam>
    public static HttpResponse<TResult?> AsJson<TResult>(this HttpResponse<String> response)
    {
        if (String.IsNullOrWhiteSpace(response.Data))
            return new HttpResponse<TResult?>(response.StatusCode, default, response.ContentType);

        var data = JsonSerializer.Deserialize<TResult>(response.Data);
        return new HttpResponse<TResult?>(response.StatusCode, data, response.ContentType)
        {
            RawResponse = response.RawResponse
        };
    }

    /// <summary>执行成功时的操作</summary>
    public static HttpResponse<T> OnSuccess<T>(this HttpResponse<T> response, Action<T?> action)
    {
        if (response.IsSuccess)
            action(response.Data);
        return response;
    }

    /// <summary>执行失败时的操作</summary>
    public static HttpResponse<T> OnFailure<T>(this HttpResponse<T> response, Action<HttpStatusCode, T?> action)
    {
        if (!response.IsSuccess)
            action(response.StatusCode, response.Data);
        return response;
    }

    /// <summary>根据状态码执行不同操作</summary>
    public static HttpResponse<T> Match<T>(
        this HttpResponse<T> response,
        Action<T?> onSuccess,
        Action<HttpStatusCode, T?> onFailure)
    {
        if (response.IsSuccess)
            onSuccess(response.Data);
        else
            onFailure(response.StatusCode, response.Data);
        return response;
    }
}
