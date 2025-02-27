using System.Text.RegularExpressions;

using NewLife;

namespace Pek.Helpers;

/// <summary>
/// 版本号帮助类
/// </summary>
public class VersionHelper
{
    public static Int32 Compare(String version1, String version2)
    {
        // 使用正则表达式将版本号分解为数字部分和字母部分
        var regex = new Regex(@"^(\d+(?:\.\d+)*)([A-Za-z]*)$");

        var match1 = regex.Match(version1);
        var match2 = regex.Match(version2);

        if (!match1.Success || !match2.Success)
            throw new ArgumentException("版本号格式不正确");

        // 获取数字部分
        var numericPart1 = match1.Groups[1].Value;
        var numericPart2 = match2.Groups[1].Value;

        // 比较数字部分
        var numericComparison = CompareNumericVersions(numericPart1, numericPart2);
        if (numericComparison != 0)
            return numericComparison;

        // 如果数字部分相同，比较字母部分
        var alphaPart1 = match1.Groups[2].Value;
        var alphaPart2 = match2.Groups[2].Value;

        // 如果一个有字母部分而另一个没有，有字母的版本更高
        if (alphaPart1.IsNullOrWhiteSpace() && !alphaPart2.IsNullOrWhiteSpace())
            return -1;

        if (!alphaPart1.IsNullOrWhiteSpace() && alphaPart2.IsNullOrWhiteSpace())
            return 1;

        // 两者都有字母，按字母顺序比较
        return String.Compare(alphaPart1, alphaPart2, StringComparison.OrdinalIgnoreCase);
    }

    private static Int32 CompareNumericVersions(String version1, String version2)
    {
        var parts1 = version1.Split('.');
        var parts2 = version2.Split('.');

        var maxLength = Math.Max(parts1.Length, parts2.Length);

        for (var i = 0; i < maxLength; i++)
        {
            // 如果一个版本比另一个短，则缺失的部分视为0
            var v1 = i < parts1.Length ? Int32.Parse(parts1[i]) : 0;
            var v2 = i < parts2.Length ? Int32.Parse(parts2[i]) : 0;

            if (v1 < v2)
                return -1;
            if (v1 > v2)
                return 1;
        }

        return 0;
    }
}
