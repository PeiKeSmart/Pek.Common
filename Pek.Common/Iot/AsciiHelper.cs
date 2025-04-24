namespace Pek.Iot;

public class AsciiHelper
{
    /// <summary>
    /// 将十六进制字符串转换为ASCII字符串
    /// </summary>
    /// <param name="hexString">十六进制字符串</param>
    /// <returns></returns>
    public static String HexStringToAsciiString(String hexString)
    {
        var bytes = new Byte[hexString.Length / 2];
        for (var i = 0; i < hexString.Length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
        }

        var asciiChars = new List<Char>();
        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] >= 32 && bytes[i] <= 126)
            {
                asciiChars.Add((Char)bytes[i]);
            }
        }

        var asciiString = new String([.. asciiChars]);
        return asciiString.TrimEnd();
    }
}
