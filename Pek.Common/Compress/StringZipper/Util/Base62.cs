using System.Text;

namespace Pek.Compress.StringZipper.Util;

public static class Base62
{
    /// <summary>
    /// Encode a byte array with Base62
    /// </summary>
    /// <param name="original">Byte array</param>
    /// <param name="inverted">Use inverted character set</param>
    /// <returns>Base62 string</returns>
    public static string ToBase62(byte[] original, bool inverted = false)
    {
        var characterSet = inverted ? "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" : "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        var array = Base62.BaseConvert(Array.ConvertAll<byte, int>(original, (byte t) => (int)t), 256, 62);
        var builder = new StringBuilder();
        foreach (var t2 in array)
        {
            builder.Append(characterSet[t2]);
        }
        return builder.ToString();
    }

    /// <summary>
    /// Decode a base62-encoded string
    /// </summary>
    /// <param name="base62">Base62 string</param>
    /// <param name="inverted">Use inverted character set</param>
    /// <returns>Byte array</returns>
    public static byte[] FromBase62(string base62, bool inverted = false)
    {
        if (string.IsNullOrWhiteSpace(base62))
        {
            throw new ArgumentNullException("base62");
        }
        var characterSet = inverted ? "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ" : "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        return Array.ConvertAll<int, byte>(Base62.BaseConvert(Array.ConvertAll<char, int>(base62.ToCharArray(), new Converter<char, int>(characterSet.IndexOf)), 62, 256), new Converter<int, byte>(Convert.ToByte));
    }

    private static int[] BaseConvert(int[] source, int sourceBase, int targetBase)
    {
        var result = new List<int>();
        var leadingZeroCount = Math.Min(source.TakeWhile((int x) => x == 0).Count<int>(), source.Length - 1);
        int count;
        while ((count = source.Length) > 0)
        {
            var quotient = new List<int>();
            var remainder = 0;
            for (var i = 0; i != count; i++)
            {
                var num = source[i] + remainder * sourceBase;
                var digit = num / targetBase;
                remainder = num % targetBase;
                if (quotient.Count > 0 || digit > 0)
                {
                    quotient.Add(digit);
                }
            }
            result.Insert(0, remainder);
            source = quotient.ToArray();
        }
        result.InsertRange(0, Enumerable.Repeat<int>(0, leadingZeroCount));
        return result.ToArray();
    }

    private const string DefaultCharacterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private const string InvertedCharacterSet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
}