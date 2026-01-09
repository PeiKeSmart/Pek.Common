using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Pek.Webs.Clients.Internal;

/// <summary>
/// HttpClient 生成工厂
/// </summary>
internal static class HttpClientBuilderFactory {
    /// <summary>
    /// HttpClient 字典
    /// </summary>
    private static readonly ConcurrentDictionary<String, HttpClient> _httpClients =
        new();

    /// <summary>
    /// 域名正则表达式
    /// </summary>
    private static readonly Regex _domainRegex =
        new(@"(http|https)://(?<domain>[^(:|/]*)", RegexOptions.IgnoreCase);

    /// <summary>
    /// 连接池生命周期（默认 2 分钟）
    /// </summary>
    private static readonly TimeSpan _pooledConnectionLifetime = TimeSpan.FromMinutes(2);

    /// <summary>
    /// 连接池空闲超时（默认 1 分钟）
    /// </summary>
    private static readonly TimeSpan _pooledConnectionIdleTimeout = TimeSpan.FromMinutes(1);

    /// <summary>
    /// 每服务器最大连接数（高并发优化）
    /// </summary>
    private static readonly Int32 _maxConnectionsPerServer = 100;

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="timeout">超时时间</param>
    public static HttpClient CreateClient(String url, TimeSpan timeout)
    {
        var domain = GetDomainByUrl(url);
        if (_httpClients.TryGetValue(domain, out var value))
            return value;
        var httpClient = Create(timeout);
        _httpClients[domain] = httpClient;
        return httpClient;
    }

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="serverCertificateCustomValidationCallback">证书回调</param>
    public static HttpClient CreateClient(String url, TimeSpan timeout, Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, Boolean>?
        serverCertificateCustomValidationCallback)
    {
        var domain = GetDomainByUrl(url);
        if (_httpClients.TryGetValue(domain, out var value))
            return value;
        var httpClient = Create(timeout, serverCertificateCustomValidationCallback);
        _httpClients[domain] = httpClient;
        return httpClient;
    }

    /// <summary>
    /// 通过Url地址获取域名
    /// </summary>
    /// <param name="url">Url地址</param>
    private static String GetDomainByUrl(String url) => _domainRegex.Match(url).Value;

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    private static HttpClient Create(TimeSpan timeout)
    {
#if NET6_0_OR_GREATER
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = _pooledConnectionLifetime,
            PooledConnectionIdleTimeout = _pooledConnectionIdleTimeout,
            MaxConnectionsPerServer = _maxConnectionsPerServer,
            UseProxy = false,
            EnableMultipleHttp2Connections = true,  // HTTP/2 多路复用
        };
#else
        var handler = new HttpClientHandler
        {
            UseProxy = false,
#if !NET462 && !NET472
            MaxConnectionsPerServer = _maxConnectionsPerServer,
#endif
        };
#endif
        
        var httpClient = new HttpClient(handler)
        {
            Timeout = timeout
        };
        //httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        return httpClient;
    }

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    private static HttpClient Create(TimeSpan timeout, Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, Boolean>?
        serverCertificateCustomValidationCallback)
    {
#if NET462 || NET472
        // .NET Framework 4.6.2 和 4.7.2 不支持 ServerCertificateCustomValidationCallback
        var handler = new HttpClientHandler()
        {
            UseProxy = false,
        };
        
        // 对于 .NET Framework，如果需要自定义证书验证，可以使用 ServicePointManager
        if (serverCertificateCustomValidationCallback != null)
        {
            // 注意：这是全局设置，可能影响其他 HttpClient 实例
            ServicePointManager.ServerCertificateValidationCallback = 
                (sender, certificate, chain, sslPolicyErrors) => 
                    serverCertificateCustomValidationCallback(null, certificate as X509Certificate2, chain, sslPolicyErrors);
        }
#elif NET6_0_OR_GREATER
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = _pooledConnectionLifetime,
            PooledConnectionIdleTimeout = _pooledConnectionIdleTimeout,
            MaxConnectionsPerServer = _maxConnectionsPerServer,
            UseProxy = false,
            EnableMultipleHttp2Connections = true,
        };
        
        // SocketsHttpHandler 使用 SslOptions 设置证书验证
        if (serverCertificateCustomValidationCallback != null)
        {
            handler.SslOptions.RemoteCertificateValidationCallback = 
                (sender, certificate, chain, sslPolicyErrors) => 
                    serverCertificateCustomValidationCallback(null, certificate as X509Certificate2, chain, sslPolicyErrors);
        }
#else
        var handler = new HttpClientHandler()
        {
            UseProxy = false,
            MaxConnectionsPerServer = _maxConnectionsPerServer,
            ServerCertificateCustomValidationCallback = serverCertificateCustomValidationCallback,
        };
#endif
        
        var httpClient = new HttpClient(handler)
        {
            Timeout = timeout,
        };
        //httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        return httpClient;
    }
}
