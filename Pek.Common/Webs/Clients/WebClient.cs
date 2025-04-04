﻿namespace Pek.Webs.Clients;

/// <summary>
/// Web客户端
/// </summary>
public class WebClient
{
    /// <summary>
    /// Get请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest Get(String url) => new HttpRequest(HttpMethod.Get, url);

    /// <summary>
    /// Post请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest Post(String url) => new HttpRequest(HttpMethod.Post, url);

    /// <summary>
    /// Put请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest Put(String url) => new HttpRequest(HttpMethod.Put, url);

    /// <summary>
    /// Delete请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest Delete(String url) => new HttpRequest(HttpMethod.Delete, url);
}

/// <summary>
/// Web客户端
/// </summary>
/// <typeparam name="TResult">返回的结果类型</typeparam>
public class WebClient<TResult> where TResult : class
{
    /// <summary>
    /// Get请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest<TResult> Get(String url) => new HttpRequest<TResult>(HttpMethod.Get, url);

    /// <summary>
    /// Post请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest<TResult> Post(String url) => new HttpRequest<TResult>(HttpMethod.Post, url);

    /// <summary>
    /// Put请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest<TResult> Put(String url) => new HttpRequest<TResult>(HttpMethod.Put, url);

    /// <summary>
    /// Delete请求
    /// </summary>
    /// <param name="url">请求地址</param>
    public IHttpRequest<TResult> Delete(String url) => new HttpRequest<TResult>(HttpMethod.Delete, url);
}