using Pek.Helpers;
using Pek.Timing;

namespace Pek.Security;

/// <summary>
/// JwtToken
/// </summary>
[Serializable]
public class JsonWebToken
{
    /// <summary>
    /// 访问令牌。用于业务身份认证的令牌
    /// </summary>
    public String AccessToken { get; set; } = String.Empty;

    /// <summary>
    /// 访问令牌有效期。UTC标准
    /// </summary>
    public Int64 AccessTokenUtcExpires { get; set; }

    /// <summary>
    /// 刷新令牌。用于刷新AccessToken的令牌
    /// </summary>
    public String RefreshToken { get; set; } = String.Empty;

    /// <summary>
    /// 刷新令牌有效期。UTC标准
    /// </summary>
    public Int64 RefreshUtcExpires { get; set; }

    /// <summary>
    /// 访问令牌签发时间。UTC标准
    /// </summary>
    public DateTimeOffset StartTime { get; set; }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public Boolean IsExpired() => Conv.CTo<long>(DateTime.UtcNow.ToJsGetTime()) > AccessTokenUtcExpires;

    /// <summary>
    /// 是否已过期
    /// </summary>
    /// <param name="Min">提前分钟数</param>
    public Boolean IsExpired(Int32 Min) => Conv.CTo<long>(DateTime.UtcNow.AddMinutes(Min).ToJsGetTime()) > AccessTokenUtcExpires;

    /// <summary>
    /// 是否刷新令牌已过期
    /// </summary>
    public Boolean IsRefreshExpired() => Conv.CTo<long>(DateTime.UtcNow.ToJsGetTime()) > RefreshUtcExpires;

    /// <summary>
    /// 是否刷新令牌已过期
    /// </summary>
    /// <param name="Min">提前分钟数</param>
    public Boolean IsRefreshExpired(Int32 Min) => Conv.CTo<long>(DateTime.UtcNow.AddMinutes(Min).ToJsGetTime()) > RefreshUtcExpires;
}