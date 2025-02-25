﻿using System.Net;
using System.Text;

namespace Pek.Webs.Clients;

/// <summary>
/// Http请求
/// </summary>
/// <typeparam name="TRequest">Http请求</typeparam>
public interface IRequest<out TRequest> where TRequest : IRequest<TRequest>
{
    /// <summary>
    /// 设置字符编码
    /// </summary>
    /// <param name="encoding">字符编码</param>
    TRequest Encoding(Encoding encoding);

    /// <summary>
    /// 设置字符编码
    /// </summary>
    /// <param name="encoding">字符编码，范例：gb2312</param>
    TRequest Encoding(String encoding);

    /// <summary>
    /// 设置内容类型
    /// </summary>
    /// <param name="contentType">内容类型</param>
    TRequest ContentType(HttpContentType contentType);

    /// <summary>
    /// 设置内容类型
    /// </summary>
    /// <param name="contentType">内容类型</param>
    TRequest ContentType(String contentType);

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="expiresDate">有效时间，单位：天</param>
    TRequest Cookie(String name, String value, Double expiresDate);

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="expiresDate">到期时间</param>
    TRequest Cookie(String name, String value, DateTime expiresDate);

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="value">值</param>
    /// <param name="path">源服务器URL子集</param>
    /// <param name="domain">所属域</param>
    /// <param name="expiresDate">到期时间</param>
    TRequest Cookie(String name, String value, String path = "/", String? domain = null, DateTime? expiresDate = null);

    /// <summary>
    /// 设置Cookie
    /// </summary>
    /// <param name="cookie">cookie</param>
    TRequest Cookie(Cookie cookie);

    /// <summary>
    /// 设置Bearer令牌
    /// </summary>
    /// <param name="token">令牌</param>
    TRequest BearerToken(String token);

    /// <summary>
    /// 设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间。单位：秒</param>
    TRequest Timeout(Int32 timeout);

    /// <summary>
    /// 设置超时时间
    /// </summary>
    /// <param name="timeout">超时时间</param>
    TRequest Timeout(TimeSpan timeout);

    /// <summary>
    /// 设置请求头
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    TRequest Header<T>(String key, T value);

    /// <summary>
    /// 添加参数字典
    /// </summary>
    /// <param name="parameters">参数字典</param>
    TRequest Data(IDictionary<String, Object> parameters);

    /// <summary>
    /// 添加参数
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    TRequest Data(String key, Object value);

    /// <summary>
    /// 添加Json参数
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="value">值</param>
    TRequest JsonData<T>(T value);

    /// <summary>
    /// 添加Xml参数
    /// </summary>
    /// <param name="value">值</param>
    TRequest XmlData(String value);

    /// <summary>
    /// 添加文件参数
    /// </summary>
    /// <param name="filePath">文件路径</param>
    TRequest FileData(String filePath);

    /// <summary>
    /// 添加文件参数
    /// </summary>
    /// <param name="name">参数名</param>
    /// <param name="filePath">文件路径</param>
    TRequest FileData(String name, String filePath);

    /// <summary>
    /// 请求失败回调函数
    /// </summary>
    /// <param name="action">执行失败的回调函数，参数为响应结果</param>
    TRequest OnFail(Action<String> action);

    /// <summary>
    /// 请求失败回调函数
    /// </summary>
    /// <param name="action">执行失败的回调函数，第一个参数为响应结果，第二个参数为状态码</param>
    TRequest OnFail(Action<String, HttpStatusCode> action);

    /// <summary>
    /// 忽略Ssl
    /// </summary>
    TRequest IgnoreSsl();

    /// <summary>
    /// 下载数据
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Byte[]?> DownloadDataAsync(CancellationToken cancellationToken = default);
}