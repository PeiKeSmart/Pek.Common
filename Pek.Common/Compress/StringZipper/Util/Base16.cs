using System.Text;

namespace Pek.Compress.StringZipper.Util;

public static class Base16
{
    public static string ToBase16(byte[] original)
    {
        var sb = new StringBuilder();
        foreach (var t in original)
        {
            sb.Append(t.ToString("X2"));
        }
        return sb.ToString();
    }

    public static byte[] FromBase16(string base16)
    {
        var bytes = new byte[base16.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(base16.Substring(i * 2, 2), 16);
        }
        return bytes;
    }
}