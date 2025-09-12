using System.Text;

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
        // 预分配字典容量以减少扩容
        var estimatedCapacity = Math.Max(256, uncompressed.Length / 4);
        var dictionary = new Dictionary<String, Int32>(estimatedCapacity);
        
        for (var i = 0; i < 256; i++)
        {
            dictionary.Add(((Char)i).ToString(), i);
        }

        var w = String.Empty;
        // 预分配结果列表容量
        var result = new List<Int32>(uncompressed.Length / 2);
        
        // 使用 StringBuilder 进行字符串拼接优化
        var stringBuilder = new StringBuilder(256);

        foreach (var c in uncompressed)
        {
            // 使用 StringBuilder 构建 wc 字符串
            stringBuilder.Clear();
            stringBuilder.Append(w).Append(c);
            var wc = stringBuilder.ToString();
                
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
        // 预分配字典容量
        var dictionary = new Dictionary<Int32, String>(compressed.Count + 256);
        for (var i = 0; i < 256; i++)
        {
            dictionary.Add(i, ((Char)i).ToString());
        }

        var firstCode = compressed[0] ^ key; // 解密
        var w = dictionary[firstCode];
        compressed.RemoveAt(0);
        
        // 使用 StringBuilder 代替 StringWriter，预分配容量
        var result = new System.Text.StringBuilder(compressed.Count * 2);
        result.Append(w);

        // 使用 StringBuilder 进行字符串拼接优化
        var stringBuilder = new StringBuilder(512);

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
                // 使用 StringBuilder 优化字符串拼接
                stringBuilder.Clear();
                stringBuilder.Append(w).Append(w[0]);
                entry = stringBuilder.ToString();
            }
            else
            {
                throw new ArgumentException("Compressed k is invalid.");
            }

            result.Append(entry);
            
            // 使用 StringBuilder 优化字典条目创建
            stringBuilder.Clear();
            stringBuilder.Append(w).Append(entry[0]);
            dictionary[dictionary.Count] = stringBuilder.ToString();
            
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
        // 预分配内存流容量
        using var ms = new MemoryStream(compressed.Count * 2);
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
        // 预分配结果列表容量
        var result = new List<Int32>(compressed.Length / 2);
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
        // 预分配字典容量，减少哈希表扩容
        var frequency = new Dictionary<Char, Int32>(Math.Min(text.Length, 256));

        // 使用 ReadOnlySpan 减少字符串枚举的开销
        var textSpan = text.AsSpan();
        for (var i = 0; i < textSpan.Length; i++)
        {
            var c = textSpan[i];
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