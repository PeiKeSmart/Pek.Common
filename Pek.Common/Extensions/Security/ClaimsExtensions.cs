using System.Security.Claims;

namespace Pek;

/// <summary>
/// 声明(<see cref="Claim"/>) 扩展
/// </summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// 尝试添加声明到列表当中。如果不存在，则添加；存在，不添加也不抛异常
    /// </summary>
    /// <param name="claims">声明列表</param>
    /// <param name="type">类型</param>
    /// <param name="value">值</param>
    /// <param name="valueType">值类型</param>
    public static void TryAddClaim(this List<Claim> claims, String type, String value,
        String valueType = ClaimValueTypes.String)
    {
        if (String.IsNullOrWhiteSpace(type))
            return;
        if (String.IsNullOrWhiteSpace(value))
            return;
        if (claims.Exists(x => x.Type.Equals(type, StringComparison.OrdinalIgnoreCase)))
            return;
        claims.Add(new Claim(type, value, valueType));
    }
}