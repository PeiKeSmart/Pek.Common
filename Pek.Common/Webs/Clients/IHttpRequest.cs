﻿using System.Net;

namespace Pek.Webs.Clients;

/// <summary>
/// Http请求
/// </summary>
public interface IHttpRequest : IRequest<IHttpRequest>
{
    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，参数为响应结果</param>
    IHttpRequest OnSuccess(Action<String> action);

    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，第一个参数为响应结果，第二个参数为状态码</param>
    IHttpRequest OnSuccess(Action<String, HttpStatusCode> action);

    /// <summary>
    /// 获取结果
    /// </summary>
    Task<String> ResultStringAsync();

    /// <summary>
    /// 获取Json结果
    /// </summary>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    Task<TResult> ResultFromJsonAsync<TResult>();

    /// <summary>
    /// 设置重试次数
    /// </summary>
    /// <param name="retryCount">重试次数</param>
    IHttpRequest Retry(Int32? retryCount);

    /// <summary>
    /// 设置异常处理函数
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <param name="func">异常处理函数</param>
    IHttpRequest WhenCatch<TException>(Func<TException, String> func) where TException : Exception;
}

/// <summary>
/// Http请求
/// </summary>
/// <typeparam name="TResult">结果类型</typeparam>
public interface IHttpRequest<TResult> : IRequest<IHttpRequest<TResult>> where TResult : class
{
    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，参数为响应结果</param>
    /// <param name="convertAction">将结果字符串转换为指定类型，当默认转换实现无法转换时使用</param>
    IHttpRequest<TResult> OnSuccess(Action<TResult?> action, Func<String, TResult>? convertAction = null);

    /// <summary>
    /// 请求成功回调函数
    /// </summary>
    /// <param name="action">执行成功的回调函数，第一个参数为响应结果，第二个参数为状态码</param>
    /// <param name="convertAction">将结果字符串转换为指定类型，当默认转换实现无法转换时使用</param>
    IHttpRequest<TResult> OnSuccess(Action<TResult?, HttpStatusCode> action, Func<String, TResult>? convertAction = null);

    /// <summary>
    /// 获取Json结果
    /// </summary>
    Task<TResult> ResultFromJsonAsync();

    /// <summary>
    /// 设置重试次数
    /// </summary>
    /// <param name="retryCount">重试次数</param>
    IHttpRequest<TResult> Retry(Int32? retryCount);

    /// <summary>
    /// 设置异常处理函数
    /// </summary>
    /// <typeparam name="TException">异常类型</typeparam>
    /// <param name="func">异常处理函数</param>
    IHttpRequest<TResult> WhenCatch<TException>(Func<TException, TResult> func) where TException : Exception;
}