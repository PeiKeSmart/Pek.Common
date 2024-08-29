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
    public static List<int> Compress(string uncompressed, int key)
    {
        var dictionary = new Dictionary<string, int>();
        for (var i = 0; i < 256; i++)
        {
            dictionary.Add(((char)i).ToString(), i);
        }

        var w = string.Empty;
        var result = new List<int>();

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

        if (!string.IsNullOrEmpty(w))
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
    public static string Decompress(IList<int> compressed, int key)
    {
        var dictionary = new Dictionary<int, string>();
        for (var i = 0; i < 256; i++)
        {
            dictionary.Add(i, ((char)i).ToString());
        }

        var firstCode = compressed[0] ^ key; // 解密
        var w = dictionary[firstCode];
        compressed.RemoveAt(0);
        var result = new StringWriter();
        result.Write(w);

        foreach (var k in compressed)
        {
            var decryptedK = k ^ key; // 解密
            string entry;
            if (dictionary.ContainsKey(decryptedK))
            {
                entry = dictionary[decryptedK];
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
    public static byte[] BitCompress(List<int> compressed)
    {
        using (var ms = new MemoryStream())
        using (var bw = new BinaryWriter(ms))
        {
            foreach (var code in compressed)
            {
                bw.Write((ushort)code); // 使用ushort存储
            }
            return ms.ToArray();
        }
    }

    /// <summary>
    /// 位解压缩
    /// </summary>
    /// <param name="compressed"></param>
    /// <returns></returns>
    public static List<int> BitDecompress(byte[] compressed)
    {
        var result = new List<int>();
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
    public static int GetKeyFromText(string text)
    {
        var frequency = new Dictionary<char, int>();

        foreach (var c in text)
        {
            if (frequency.ContainsKey(c))
                frequency[c]++;
            else
                frequency[c] = 1;
        }

        var top3 = frequency
                    .OrderByDescending(kvp => kvp.Value) // 按频次降序排列
                    .ThenBy(kvp => kvp.Key) // 如果频次相同则按ASCII码升序排列
                    .Take(3)
                    .Select(kvp => (int)kvp.Key)
                    .ToArray();

        return top3.Sum();
    }
}