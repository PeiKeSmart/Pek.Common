namespace Pek.Compress;

/// <summary>
/// 海凌科物联网数据压缩算法
/// </summary>
public class DHLZW
{
    /// <summary>
    /// 压缩
    /// </summary>
    /// <param name="uncompressed"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static List<Int32> Compress(String uncompressed, Int32 key)
    {
        var dictionary = new Dictionary<String, Int32>();
        for (var i = 0; i < 256; i++)
        {
            dictionary.Add(((Char)i).ToString(), i);
        }

        var w = String.Empty;
        var result = new List<Int32>();

        foreach (var c in uncompressed)
        {
            var wc = w + c;
            if (dictionary.ContainsKey(wc))
            {
                w = wc;
            }
            else
            {
                result.Add(dictionary[w] ^ key); // 加密
                dictionary[wc] = dictionary.Count;
                w = c.ToString();
            }
        }

        if (!String.IsNullOrEmpty(w))
        {
            result.Add(dictionary[w] ^ key); // 加密
        }

        return result;
    }

    /// <summary>
    /// 解压缩
    /// </summary>
    /// <param name="compressed"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static String Decompress(List<Int32> compressed, Int32 key)
    {
        var dictionary = new Dictionary<Int32, String>();
        for (var i = 0; i < 256; i++)
        {
            dictionary.Add(i, ((Char)i).ToString());
        }

        var firstCode = compressed[0] ^ key; // 解密
        var w = dictionary[firstCode];
        compressed.RemoveAt(0);
        var result = new StringWriter();
        result.Write(w);

        foreach (var k in compressed)
        {
            var decryptedK = k ^ key; // 解密
            String entry;
            if (dictionary.TryGetValue(decryptedK, out var value))
            {
                entry = value;
            }
            else if (decryptedK == dictionary.Count)
            {
                entry = w + w[0];
            }
            else
            {
                throw new ArgumentException("Compressed k is invalid.");
            }

            result.Write(entry);
            dictionary[dictionary.Count] = w + entry[0];
            w = entry;
        }

        return result.ToString();
    }

    /// <summary>
    /// 位压缩
    /// </summary>
    /// <param name="compressed"></param>
    /// <returns></returns>
    public static Byte[] BitCompress(List<Int32> compressed)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        foreach (var code in compressed)
        {
            bw.Write((UInt16)code); // 使用ushort存储
        }
        return ms.ToArray();
    }

    /// <summary>
    /// 位解压缩
    /// </summary>
    /// <param name="compressed"></param>
    /// <returns></returns>
    public static List<Int32> BitDecompress(Byte[] compressed)
    {
        var result = new List<Int32>();
        using (var ms = new MemoryStream(compressed))
        using (var br = new BinaryReader(ms))
        {
            while (br.BaseStream.Position != br.BaseStream.Length)
            {
                result.Add(br.ReadUInt16()); // 读取ushort
            }
        }
        return result;
    }

    /// <summary>
    /// 取文本的前三个最高频字符的ASCII码之和作为密钥
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Int32 GetKeyFromText(String text)
    {
        var frequency = new Dictionary<Char, Int32>();

        foreach (var c in text)
        {
            if (frequency.TryGetValue(c, out var value))
                frequency[c] = ++value;
            else
                frequency[c] = 1;
        }

        var top3 = frequency
                    .OrderByDescending(kvp => kvp.Value) // 按频次降序排列
                    .ThenBy(kvp => kvp.Key) // 如果频次相同则按ASCII码升序排列
                    .Take(3)
                    .Select(kvp => (Int32)kvp.Key)
                    .ToArray();

        return top3.Sum();
    }
}