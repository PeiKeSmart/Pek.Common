using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Pek.Helpers;

/// <summary>
/// 颜色转换器
/// </summary>
public static class ColorConverter
{
    /// <summary>
    /// 转换为16进制颜色
    /// </summary>
    /// <param name="color">颜色</param>
    /// <returns></returns>
    public static String ToHex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    /// <summary>
    /// 转换为RGB颜色
    /// </summary>
    /// <param name="color">颜色</param>
    public static String ToRgb(Color color) => $"RGB({color.R},{color.G},{color.B})";

    /// <summary>
    /// RGB格式转换为16进制颜色
    /// </summary>
    /// <param name="r">红色</param>
    /// <param name="g">绿色</param>
    /// <param name="b">蓝色</param>
    public static String RgbToHex(Int32 r, Int32 g, Int32 b) => ToHex(Color.FromArgb(r, g, b));

    /// <summary>
    /// 从样式颜色中获取系统颜色
    /// </summary>
    /// <param name="cssColour">样式颜色</param>
    public static Color GetColorFromCssString(String cssColour)
    {
        if (String.IsNullOrWhiteSpace(cssColour))
        {
            throw new ArgumentNullException(nameof(cssColour));
        }

        var m1 = Regex.Match(cssColour, @"^#?([A-F\d]{2})([A-F\d]{2})([A-F\d]{2})([A-F\d]{2})?",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);// #FFFFFF
        if (m1.Success && m1.Groups.Count == 5)
        {
            if (m1.Groups[4].Value.Length > 0)// 判断是否包含透明度
            {
                return Color.FromArgb(Byte.Parse(m1.Groups[1].Value, NumberStyles.HexNumber),
                    Byte.Parse(m1.Groups[2].Value, NumberStyles.HexNumber),
                    Byte.Parse(m1.Groups[3].Value, NumberStyles.HexNumber),
                    Byte.Parse(m1.Groups[4].Value, NumberStyles.HexNumber));
            }

            return Color.FromArgb(0xFF,
                Byte.Parse(m1.Groups[1].Value, NumberStyles.HexNumber),
                Byte.Parse(m1.Groups[2].Value, NumberStyles.HexNumber),
                Byte.Parse(m1.Groups[3].Value, NumberStyles.HexNumber));
        }
        else
        {
            var m2 = Regex.Match(cssColour, @"^#?([A-F\d])([A-F\d])([A-F\d])$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);// #FFF
            if (m2.Success && m2.Groups.Count == 4)
            {
                var r = Byte.Parse(m2.Groups[1].Value, NumberStyles.HexNumber);
                r += (Byte)(r << 4);
                var g = Byte.Parse(m2.Groups[2].Value, NumberStyles.HexNumber);
                g += (Byte)(g << 4);
                var b = Byte.Parse(m2.Groups[3].Value, NumberStyles.HexNumber);
                b += (Byte)(b << 4);
                return Color.FromArgb(0xFF, r, g, b);
            }

            if (cssColour.StartsWith("rgb(") && cssColour.EndsWith(")"))
            {
                var rgbTemp = cssColour.Remove(cssColour.Length - 1).Remove(0, "rgb(".Length).Split(',');

                if (rgbTemp.Length == 3)
                {
                    var r = ParseRgb(rgbTemp[0]);
                    var g = ParseRgb(rgbTemp[1]);
                    var b = ParseRgb(rgbTemp[2]);
                    return Color.FromArgb(0xFF, r, g, b);
                }
            }

            if (cssColour.StartsWith("rgba(") && cssColour.EndsWith(")"))
            {
                var rgbaTemp = cssColour.Remove(cssColour.Length - 1).Remove(0, "rgba(".Length).Split(',');

                if (rgbaTemp.Length == 3)
                {
                    var r = ParseRgb(rgbaTemp[0]);
                    var g = ParseRgb(rgbaTemp[1]);
                    var b = ParseRgb(rgbaTemp[2]);
                    var a = ParseFloat(rgbaTemp[3]);
                    return Color.FromArgb(a, r, g, b);
                }
            }

            if (cssColour.StartsWith("hsl(") && cssColour.EndsWith(")"))
            {
                var hslTemp = cssColour.Remove(cssColour.Length - 1).Remove(0, "hsl(".Length).Split(',');

                if (hslTemp.Length == 3)
                {
                    var h = ParseHue(hslTemp[0]);
                    var s = ParseFloat(hslTemp[1]);
                    var l = ParseFloat(hslTemp[2]);
                    return HslaToRgba(h, s, l);
                }
            }

            if (cssColour.StartsWith("hsla(") && cssColour.EndsWith(")"))
            {
                var hslaTemp = cssColour.Remove(cssColour.Length - 1).Remove(0, "hsla(".Length).Split(',');

                if (hslaTemp.Length == 4)
                {
                    var h = ParseHue(hslaTemp[0]);
                    var s = ParseFloat(hslaTemp[1]);
                    var l = ParseFloat(hslaTemp[2]);
                    var a = ParseFloat(hslaTemp[3]);
                    return HslaToRgba(h, s, l, a);
                }
            }

            switch (cssColour.ToLower())
            {
                case "white":
                case "silver":
                case "gray":
                case "black":
                case "red":
                case "maroon":
                case "yellow":
                case "olive":
                case "lime":
                case "green":
                case "aqua":
                case "teal":
                case "blue":
                case "navy":
                case "fuschia":
                case "purple":
                    return Color.FromName(cssColour);

                default:
                    throw new ArgumentException("无效颜色");
            }
        }
    }

    /// <summary>
    /// Hsla格式转换为RGBA格式
    /// </summary>
    /// <param name="hue"></param>
    /// <param name="saturation"></param>
    /// <param name="lightness"></param>
    /// <param name="alpha"></param>
    public static Color HslaToRgba(Int16 hue, Byte saturation, Byte lightness, Byte alpha = 255)
    {
        Double h = hue / 360.0f;
        Double sl = saturation / 255.0f;
        Double l = lightness / 255.0f;

        var r = l;
        var g = l;
        var b = l;

        var v = (l <= 0.5) ? (1 * (1.0 + sl)) : (l + sl - l * sl);

        if (v > 0)
        {
            var m = l + l - v;
            var sv = (v - m) / v;
            h *= 6.0;
            var sextant = (Int32)h;
            var fract = h - sextant;
            var vsf = v * sv * fract;
            var mid1 = m + vsf;
            var mid2 = v - vsf;

            switch (sextant)
            {
                case 0:
                    r = v;
                    g = mid1;
                    b = m;
                    break;

                case 1:
                    r = mid2;
                    g = v;
                    b = m;
                    break;

                case 2:
                    r = m;
                    g = v;
                    b = mid1;
                    break;

                case 3:
                    r = m;
                    g = mid2;
                    b = v;
                    break;

                case 4:
                    r = mid1;
                    g = m;
                    b = v;
                    break;

                case 5:
                    r = v;
                    g = m;
                    b = mid2;
                    break;
            }
        }

        return Color.FromArgb(alpha, Convert.ToByte(r * 255.0f), Convert.ToByte(g * 255.0f),
            Convert.ToByte(b * 255.0f));
    }

    /// <summary>
    /// 格式化RGB
    /// </summary>
    /// <param name="input"></param>
    private static Byte ParseRgb(String input)
    {
        var parseString = input.Trim();
        if (parseString.EndsWith("%"))
        {
            return (Byte)(ParseClamp(parseString.Remove(parseString.Length - 1), 100) * 2.55);
        }

        return (Byte)(ParseClamp(parseString, 255));
    }

    /// <summary>
    /// 格式化范围值
    /// </summary>
    /// <param name="input"></param>
    /// <param name="maxValue"></param>
    /// <param name="minValue"></param>
    private static Double ParseClamp(String input, Double maxValue, Double minValue = 0)
    {
        if (Double.TryParse(input, out var parsedValue))
        {
            if (parsedValue > maxValue)
            {
                return maxValue;
            }

            if (parsedValue < minValue)
            {
                return minValue;
            }

            return parsedValue;
        }
        throw new ArgumentException($"无效数字 \"{input}\"");
    }

    /// <summary>
    /// 格式化Float
    /// </summary>
    /// <param name="input"></param>
    private static Byte ParseFloat(String input)
    {
        var parseString = input.Trim();
        if (parseString.EndsWith("%"))
        {
            return (Byte)(ParseClamp(parseString.Remove(parseString.Length - 1), 100) * 2.55);
        }

        return (Byte)(ParseClamp(parseString, 1) * 255);
    }

    /// <summary>
    /// 格式化Hue
    /// </summary>
    /// <param name="input"></param>
    private static Int16 ParseHue(String input)
    {
        if (Double.TryParse(input, out var parsedValue))
        {
            return (Int16)(((parsedValue % 360) + 360) % 360);
        }
        throw new ArgumentException($"无效数字 \"{input}\"");
    }
}