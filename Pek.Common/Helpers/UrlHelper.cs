using NewLife.Collections;

namespace Pek;

/// <summary>
/// Url操作
/// </summary>
public static partial class UrlHelper
{
    #region Combine(合并Url)

    /// <summary>
    /// 合并Url，支持包含"/"符号
    /// </summary>
    /// <param name="urls">url片段，范例：Url.Combine( "http://a.com","b" ),返回 "http://a.com/b"</param>
    public static String Combine(params String[] urls)
    {
        if (urls == null || urls.Length == 0) return String.Empty;

        var result = Pool.StringBuilder.Get();
        foreach (var url in urls)
        {
            if (result.Length > 0 && !result.ToString().EndsWith("/"))
            {
                result.Append('/');
            }
            result.Append(url.Trim('/'));
        }

        return result.Return(true);
    }

    #endregion

    #region Join(连接Url)

    /// <summary>
    /// 连接Url，范例：Url.Join( "http://a.com","b=1" ),返回 "http://a.com?b=1"
    /// </summary>
    /// <param name="url">Url，范例：http://a.com</param>
    /// <param name="param">参数，范例：b=1</param>
    public static String Join(String url, String param) => $"{GetUrl(url)}{param}";

    /// <summary>
    /// 连接Url，范例：Url.Join( "http://a.com",new []{"b=1","c=2"})，返回"http://a.com?b=1&amp;c=2"
    /// </summary>
    /// <param name="url">Url，范例：http://a.com</param>
    /// <param name="parameters">参数，范例：b=1</param>
    public static String Join(String url, params String[] parameters)
    {
        if (String.IsNullOrWhiteSpace(url))
            throw new ArgumentNullException(nameof(url));
        if (parameters.Length == 0)
            return url;
        var currentUrl = Join(url, parameters[0]);
        return Join(currentUrl, parameters.Skip(1).ToArray());
    }

    /// <summary>
    /// 获取Url
    /// </summary>
    /// <param name="url">Url，范例：http://a.com</param>
    private static String GetUrl(String url)
    {
        if (!url.Contains('?'))
            return $"{url}?";
        if (url.EndsWith("?"))
            return url;
        if (url.EndsWith("&"))
            return url;
        return $"{url}&";
    }

    /// <summary>
    /// 连接Url，范例：Url.Join( "http://a.com","b=1" ),返回 "http://a.com?b=1"
    /// </summary>
    /// <param name="url">Url，范例：http://a.com</param>
    /// <param name="param">参数，范例：b=1</param>
    public static Uri Join(Uri url, String param) => url == null ? throw new ArgumentNullException(nameof(url)) : new Uri(Join(url.AbsoluteUri, param));

    /// <summary>
    /// 连接Url，范例：Url.Join( "http://a.com",new []{"b=1","c=2"})，返回"http://a.com?b=1&amp;c=2"
    /// </summary>
    /// <param name="url">Url，范例：http://a.com</param>
    /// <param name="parameters">参数，范例：b=1</param>
    public static Uri Join(Uri url, params String[] parameters) => url == null ? throw new ArgumentNullException(nameof(url)) : new Uri(Join(url.AbsoluteUri, parameters));

    #endregion

    #region GetMainDomain(获取主域名)

    /// <summary>
    /// 获取主域名
    /// </summary>
    /// <param name="url">Url地址</param>
    public static String GetMainDomain(String url)
    {
        if (String.IsNullOrWhiteSpace(url))
            return url;
        var array = url.Split('.');
        if (array.Length != 3)
            return url;
        var tok = new List<String>(array);
        var remove = array.Length - 2;
        tok.RemoveRange(0, remove);
        return tok[0] + "." + tok[1];
    }

    #endregion
}