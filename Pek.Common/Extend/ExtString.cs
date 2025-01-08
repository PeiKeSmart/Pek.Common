using System.Text;
using System.Text.RegularExpressions;

namespace Pek;

public static class ExtString
{
    public static String NoHTML(this String Htmlstring)
    {
        Htmlstring = Regex.Replace(Htmlstring, @"<script[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<noscript[\s\S]*?</noscript>", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<style[\s\S]*?</style>", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<.*?>", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<(.[^>]*)>", " ", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", " ", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"-->", " ", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"<!--.*", " ", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(nbsp|#160);", "", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
        Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", " ", RegexOptions.IgnoreCase);
        return Htmlstring;
    }

    public static Byte[] ToByte(this String value) => Encoding.UTF8.GetBytes(value);

    public static String UrlEncode(this String value)
    {
        var sb = new StringBuilder();
        var byStr = Encoding.UTF8.GetBytes(value);
        for (var i = 0; i < byStr.Length; i++)
        {
            sb.Append(@"%" + Convert.ToString(byStr[i], 16));
        }
        return (sb.ToString());
    }

    public static String ToUnicode(this String value)
    {
        if (String.IsNullOrEmpty(value)) return value;
        var builder = new StringBuilder();
        for (var i = 0; i < value.Length; i++)
        {
            builder.Append("\\u" + ((Int32)value[i]).ToString("x"));
        }
        return builder.ToString();
    }

    private static readonly Regex emailExpression = new(@"^([0-9a-zA-Z]+[-._+&])*[0-9a-zA-Z]+@([-0-9a-zA-Z]+[.])+[a-zA-Z]{2,6}$", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex webUrlExpression = new(@"(http|https)://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?", RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Regex stripHTMLExpression = new("<\\S[^><]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly Char[] separator = new Char[] { '/', '\\' };

    public static String FormatWith(this String instance, params Object[] args) => String.Format(instance, args);

    public static T ToEnum<T>(this String instance, T defaultValue) where T : struct, IComparable, IFormattable
    {
        var convertedValue = defaultValue;

        if (!String.IsNullOrWhiteSpace(instance) && !Enum.TryParse(instance.Trim(), true, out convertedValue))
        {
            convertedValue = defaultValue;
        }

        return convertedValue;
    }

    public static T ToEnum<T>(this Int32 instance, T defaultValue) where T : struct, IComparable, IFormattable
    {
        if (!Enum.TryParse(instance.ToString(), true, out T convertedValue))
        {
            convertedValue = defaultValue;
        }

        return convertedValue;
    }

    public static String StripHtml(this String instance) => stripHTMLExpression.Replace(instance, String.Empty);

    public static Boolean IsEmail(this String instance) => !String.IsNullOrWhiteSpace(instance) && emailExpression.IsMatch(instance);

    public static Boolean IsWebUrl(this String instance) => !String.IsNullOrWhiteSpace(instance) && webUrlExpression.IsMatch(instance);

    public static Boolean AsBool(this String instance)
    {
        _ = Boolean.TryParse(instance, out var result);
        return result;
    }

    public static DateTime AsDateTime(this String instance)
    {
        _ = DateTime.TryParse(instance, out var result);
        return result;
    }

    public static Decimal AsDecimal(this String instance)
    {
        _ = Decimal.TryParse(instance, out var result);
        return result;
    }

    public static Int32 AsInt(this String instance)
    {
        _ = Int32.TryParse(instance, out var result);
        return result;
    }

    public static Boolean IsIntT(this String instance) => Int32.TryParse(instance, out _);

    public static Boolean IsFloat(this String instance) => Single.TryParse(instance, out _);

    public static String FirstCharToLowerCase(this String instance)
    {
        if (!String.IsNullOrWhiteSpace(instance) && instance.Length > 2 && Char.IsUpper(instance[0]))
        {
            return Char.ToLower(instance[0]) + instance[1..];
        }
        if (instance.Length == 2)
        {
            return instance.ToLower();
        }
        return instance;
    }

    public static String ToFilePath(this String path) => Path.Combine(path.Split(separator, StringSplitOptions.RemoveEmptyEntries));

    public static String CombinePath(this String p, String path) => $"{p.TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}{path.ToFilePath()}";
}