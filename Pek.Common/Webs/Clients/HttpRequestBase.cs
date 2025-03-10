﻿using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using NewLife.Log;
using NewLife.Serialization;

using Pek.Webs.Clients.Internal;
using Pek.Webs.Clients.Parameters;

namespace Pek.Webs.Clients;

/// <summary>
/// Http请求基类
/// </summary>
/// <typeparam name="TRequest">Http请求</typeparam>
public abstract class HttpRequestBase<TRequest> where TRequest : IRequest<TRequest>
{
    #region 字段

    /// <summary>
    /// 请求地址
    /// </summary>
    private readonly String _url;

    /// <summary>
    /// Http请求方法
    /// </summary>
    private readonly HttpMethod _httpMethod;

    /// <summary>
    /// 参数集合
    /// </summary>
    private IDictionary<String, Object> _params;

    /// <summary>
    /// 参数
    /// </summary>
    private String? _data;

    /// <summary>
    /// 字符编码
    /// </summary>
    private Encoding _encoding;

    /// <summary>
    /// 内容类型
    /// </summary>
    private String _contentType;

    /// <summary>
    /// Cookie容器
    /// </summary>
    private readonly CookieContainer _cookieContainer;

    /// <summary>
    /// 超时时间
    /// </summary>
    private TimeSpan _timeout;

    /// <summary>
    /// 请求头集合
    /// </summary>
    private readonly Dictionary<String, String> _headers;

    /// <summary>
    /// 执行失败的回调函数
    /// </summary>
    private Action<String>? _failAction;

    /// <summary>
    /// 执行失败的回调函数
    /// </summary>
    private Action<String, HttpStatusCode>? _failStatusCodeAction;

    /// <summary>
    /// ssl证书验证委托
    /// </summary>
    private Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, Boolean>?
        _serverCertificateCustomValidationCallback;

    /// <summary>
    /// 令牌
    /// </summary>
    private String? _token;

    /// <summary>
    /// 文件集合
    /// </summary>
    private readonly IList<IFileParameter> _files;

    /// <summary>
    /// 重试次数
    /// </summary>
    protected Int32 _retryCount;

    protected String? Url => _url;

    #endregion

    #region 构造函数

    /// <summary>
    /// 初始化一个<see cref="HttpRequestBase{TRequest}"/>类型的实例
    /// </summary>
    /// <param name="httpMethod">Http请求方法</param>
    /// <param name="url">请求地址</param>
    protected HttpRequestBase(HttpMethod httpMethod, String url)
    {
        if (String.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentNullException(nameof(url));
        }

#if NETCOREAPP
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);// 解决当前.net框架未支持的未知字符编码类型
#endif
        _url = url;
        _httpMethod = httpMethod;
        _params = new Dictionary<String, Object>();
        _contentType = HttpContentType.FormUrlEncoded.Description();
        _cookieContainer = new CookieContainer();
        _timeout = new TimeSpan(0, 0, 30);
        _headers = [];
        _encoding = System.Text.Encoding.UTF8;
        _files = [];
    }

    /// <summary>
    /// 返回自身
    /// </summary>
    private TRequest This() => (TRequest)(Object)this;

#endregion

    #region Encoding(设置字符编码)

    /// <summary>
    /// 设置字符编码
    /// </summary>
    /// <param name="encoding">字符编码</param>
    public TRequest Encoding(Encoding encoding)
    {
        _encoding = encoding;
        return This();
    }

    /// <summary>
    /// 设置字符编码
    /// </summary>
    /// <param name="encoding">字符编码</param>
    public TRequest Encoding(String encoding) => Encoding(System.Text.Encoding.GetEncoding(encoding));

    #endregion

    #region ContentType(设置内容类型)

    /// <summary>
    /// 设置内容类型
    /// </summary>
    /// <param name="contentType">内容类型</param>
    public TRequest ContentType(HttpContentType contentType) => ContentType(contentType.Description());

    /// <summary>
    /// 设置内容类型
    /// </summary>
    /// <param name="contentType">内容类型</param>
    public TRequest ContentType(String contentType)
    {
        _contentType = contentType;
        return This();
    }

    #endregion

    #region Cookie(设置Cookie)

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="expiresDate">有效时间，单位：天</param>
    public TRequest Cookie(String name, String value, Double expiresDate) => Cookie(name, value, null, null, DateTime.Now.AddDays(expiresDate));

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="expiresDate">到期时间</param>
    public TRequest Cookie(String name, String value, DateTime expiresDate) => Cookie(name, value, null, null, expiresDate);

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="path">源服务器URL子集</param>
    /// <param name="domain">所属域</param>
    /// <param name="expiresDate">到期时间</param>
    public TRequest Cookie(String name, String value, String? path = "/", String? domain = null,
        DateTime? expiresDate = null) =>
        Cookie(new Cookie(name, value, path, domain) { Expires = expiresDate ?? DateTime.Now.AddYears(1) });

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="cookie">cookie</param>
    public TRequest Cookie(Cookie cookie)
    {
        _cookieContainer.Add(new Uri(_url), cookie);
        return This();
    }

    #endregion

    #region Timeout(设置超时时间)

    /// <summary>
    /// 设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间。单位：秒</param>
    public TRequest Timeout(Int32 timeout) => Timeout(new TimeSpan(0, 0, timeout));

    /// <summary>
    /// 设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间</param>
    public TRequest Timeout(TimeSpan timeout)
    {
        _timeout = timeout;
        return This();
    }

    #endregion

    #region Header(设置请求头)

    /// <summary>
    /// 设置请求头
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    public TRequest Header<T>(String key, T value)
    {
        _headers.Add(key, value.SafeString());
        return This();
    }

    #endregion

    #region Data(添加参数)

    /// <summary>
    /// 添加参数字典
    /// </summary>
    /// <param name="parameters">参数字典</param>
    public TRequest Data(IDictionary<String, Object> parameters)
    {
        _params = parameters ?? throw new ArgumentNullException(nameof(parameters));
        return This();
    }

    /// <summary>
    /// 添加参数
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    public TRequest Data(String key, Object value)
    {
        if (String.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        if (String.IsNullOrWhiteSpace(value.SafeString()))
            return This();
        _params.Add(key, value);
        return This();
    }

    /// <summary>
    /// 添加Json参数
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="value">值</param>
    public TRequest JsonData<T>(T value)
    {
        if (value == null)
            return This();

        ContentType(HttpContentType.Json);
        _data = JsonHelper.ToJson(value);
        return This();
    }

    /// <summary>
    /// 添加Xml参数
    /// </summary>
    /// <param name="value">值</param>
    public TRequest XmlData(String value)
    {
        ContentType(HttpContentType.Xml);
        _data = value;
        return This();
    }

    /// <summary>
    /// 添加文件参数
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public TRequest FileData(String filePath) => FileData("files", filePath);

    /// <summary>
    /// 添加文件参数
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="filePath">文件路径</param>
    public TRequest FileData(String name, String filePath)
    {
        ContentType(HttpContentType.FormData);
        _files.Add(new PhysicalFileParameter(filePath, name));
        return This();
    }

    #endregion

    #region OnFail(请求失败回调函数)

    /// <summary>
    /// 请求失败回调函数
    /// </summary>
    /// <param name="action">执行失败的回调函数，参数为响应结果</param>
    public TRequest OnFail(Action<String> action)
    {
        _failAction = action;
        return This();
    }

    /// <summary>
    /// 请求失败回调函数
    /// </summary>
    /// <param name="action">执行失败的回调函数，第一个参数为响应结果，第二个参数为状态码</param>
    public TRequest OnFail(Action<String, HttpStatusCode> action)
    {
        _failStatusCodeAction = action;
        return This();
    }

    #endregion

    #region IgnoreSsl(忽略Ssl)

    /// <summary>
    /// 忽略Ssl
    /// </summary>
    public TRequest IgnoreSsl()
    {
        _serverCertificateCustomValidationCallback = (a, b, c, d) => true;
        return This();
    }

    #endregion

    #region BearerToken(设置Bearer令牌)

    /// <summary>
    /// 设置Bearer令牌
    /// </summary>
    /// <param name="token">令牌</param>
    public TRequest BearerToken(String token)
    {
        _token = token;
        return This();
    }

    #endregion

    #region ResultAsync(获取结果)

    /// <summary>
    /// 获取结果
    /// </summary>
    protected async Task<String> ResultAsync()
    {
        SendBefore();
        var response = await SendAsync().ConfigureAwait(false);
        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        SendAfter(result, response);
        return result;
    }

    #endregion

    #region DownloadDataAsync(下载)

    /// <summary>
    /// 下载
    /// </summary>
    public async Task<Byte[]?> DownloadDataAsync(CancellationToken cancellationToken = default)
    {
        SendBefore();
        var response = await SendAsync().ConfigureAwait(false);

#if NETCOREAPP
        var result = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
        var result = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#endif
        if (result == null)
            return null;

        //SendAfter(result, response);

        return await result.GetAllBytesAsync(cancellationToken).ConfigureAwait(false);
    }

#endregion

    #region SendBefore(发送前操作)

    /// <summary>
    /// 发送前操作
    /// </summary>
    public virtual void SendBefore()
    {
    }

    #endregion

    #region SendAsync(发送请求)

    /// <summary>
    /// 发送请求
    /// </summary>
    protected async Task<HttpResponseMessage> SendAsync()
    {
        var client = CreateHttpClient();
        InitHttpClient(client);
        return await client.SendAsync(CreateRequestMessage()).ConfigureAwait(false);
    }

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    protected virtual HttpClient CreateHttpClient()
    {
        var client = HttpClientBuilderFactory.CreateClient(_url, _timeout, _serverCertificateCustomValidationCallback);
        

        return client;
        //return new HttpClient(new HttpClientHandler()//{//    CookieContainer = _cookieContainer,//    ServerCertificateCustomValidationCallback = _serverCertificateCustomValidationCallback//})//{ Timeout = _timeout };
    }

    /// <summary>
    /// 初始化Http客户端
    /// </summary>
    /// <param name="client">Http客户端</param>
    protected virtual void InitHttpClient(HttpClient client)
    {
        if (String.IsNullOrWhiteSpace(_token))
            return;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    /// <summary>
    /// 创建请求消息
    /// </summary>
    /// <returns></returns>
    protected virtual HttpRequestMessage CreateRequestMessage()
    {
        var message = new HttpRequestMessage()
        {
            Method = _httpMethod,
            RequestUri = new Uri(_url),
            Content = CreateHttpContent()
        };
        foreach (var header in _headers)
            message.Headers.Add(header.Key, header.Value);
        return message;
    }

    /// <summary>
    /// 创建请求内容
    /// </summary>
    private HttpContent CreateHttpContent()
    {
        var contentType = _contentType.SafeString().ToLower();
        switch (contentType)
        {
            case "application/x-www-form-urlencoded":
                return new FormUrlEncodedContent(_params.ToDictionary(t => t.Key, t => t.Value.SafeString()));

            case "application/json":
                return CreateJsonContent();

            case "text/xml":
                return CreateXmlContent();

            case "multipart/form-data":
                return CreateMultipartFormDataContent();
            default:
                break;
        }
        throw new NotImplementedException($"未实现该 '{contentType}' ContentType");
    }

    /// <summary>
    /// 创建Json内容
    /// </summary>
    private StringContent CreateJsonContent()
    {
        if (String.IsNullOrWhiteSpace(_data))
            _data = JsonHelper.ToJson(_params);
        return new StringContent(_data, _encoding, "application/json");
    }

    /// <summary>
    /// 创建Xml内容
    /// </summary>
    private StringContent CreateXmlContent() => new(_data!, _encoding, "text/xml");

    /// <summary>
    /// 创建表单内容
    /// </summary>
    private MultipartFormDataContent CreateMultipartFormDataContent()
    {
        var content =
            new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture));
        foreach (var file in _files)
            content.Add(new StreamContent(file.GetFileStream()), file.GetName(), file.GetFileName());
        foreach (var item in _params)
            content.Add(new StringContent(item.Value.SafeString()), item.Key);
        return content;
    }

    #endregion

    #region SendAfter(发送后操作)

    /// <summary>
    /// 发送后操作
    /// </summary>
    /// <param name="result">结果</param>
    /// <param name="response">Http响应消息</param>
    protected virtual void SendAfter(String result, HttpResponseMessage response)
    {
        var contentType = HttpRequestBase<TRequest>.GetContentType(response);
        if (response.IsSuccessStatusCode)
        {
            SuccessHandler(result, response.StatusCode, contentType);
            return;
        }
        FailHandler(result, response.StatusCode, contentType);
    }

    /// <summary>
    /// 获取内容类型
    /// </summary>
    /// <param name="response">Http响应消息</param>
    private static String? GetContentType(HttpResponseMessage response)
    {
        return response?.Content?.Headers?.ContentType == null
            ? String.Empty
            : response.Content.Headers.ContentType.MediaType;
    }

    /// <summary>
    /// 成功处理操作
    /// </summary>
    /// <param name="result">结果</param>
    /// <param name="statusCode">状态码</param>
    /// <param name="contentType">内容类型</param>
    protected virtual void SuccessHandler(String result, HttpStatusCode statusCode, String? contentType)
    {
    }

    /// <summary>
    /// 失败处理操作
    /// </summary>
    /// <param name="result">结果</param>
    /// <param name="statusCode">状态码</param>
    /// <param name="contentType">内容类型</param>
    protected virtual void FailHandler(String result, HttpStatusCode statusCode, String? contentType)
    {
        _failAction?.Invoke(result);
        _failStatusCodeAction?.Invoke(result, statusCode);
    }

    #endregion

    /// <summary>
    /// 设置重试次数
    /// </summary>
    /// <param name="retryCount">重试次数</param>
    public TRequest Retry(Int32? retryCount)
    {
        _retryCount = retryCount ?? 0;
        return This();
    }
}