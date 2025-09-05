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
        
        // 使用栈分配的字符缓冲区，避免堆分配
        Span<char> charBuffer = stackalloc char[256];
        
        // 使用 ReadOnlySpan 遍历输入字符串，减少字符串枚举开销
        var textSpan = uncompressed.AsSpan();

        for (var i = 0; i < textSpan.Length; i++)
        {
            var c = textSpan[i];
            
            // 构建 wc 字符串，使用 Span 避免不必要的分配
            var wLength = w.Length;
            var wcLength = wLength + 1;
            
            if (wcLength <= charBuffer.Length)
            {
                // 将 w 复制到缓冲区
                w.AsSpan().CopyTo(charBuffer);
                // 添加当前字符
                charBuffer[wLength] = c;
                
                // 创建 wc 字符串（只在必要时分配）
                var wc = new string(charBuffer[..wcLength]);
                
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
            else
            {
                // 回退到原始方式处理超长字符串
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

        // 使用栈分配的字符缓冲区用于字符串拼接
        Span<char> tempBuffer = stackalloc char[512];

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
                // 使用 Span 优化字符串拼接
                var wSpan = w.AsSpan();
                var entryLength = wSpan.Length + 1;
                
                if (entryLength <= tempBuffer.Length)
                {
                    wSpan.CopyTo(tempBuffer);
                    tempBuffer[wSpan.Length] = w[0];
                    entry = new string(tempBuffer[..entryLength]);
                }
                else
                {
                    // 回退到原始方式处理超长字符串
                    entry = w + w[0];
                }
            }
            else
            {
                throw new ArgumentException("Compressed k is invalid.");
            }

            result.Append(entry);
            
            // 使用 Span 优化字典条目创建
            var wSpanForDict = w.AsSpan();
            var dictEntryLength = wSpanForDict.Length + 1;
            
            if (dictEntryLength <= tempBuffer.Length)
            {
                wSpanForDict.CopyTo(tempBuffer);
                tempBuffer[wSpanForDict.Length] = entry[0];
                dictionary[dictionary.Count] = new string(tempBuffer[..dictEntryLength]);
            }
            else
            {
                // 回退到原始方式处理超长字符串
                dictionary[dictionary.Count] = w + entry[0];
            }
            
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