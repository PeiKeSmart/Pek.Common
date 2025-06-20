﻿using System.Text;

using Murmur;

using NewLife;

namespace Pek.Ids;

/// <summary>
/// 生成短惟一码
/// </summary>
public class ShortUniqueCode
{
    public static String CreateCode(Int32 Id, Int32 Length = 6)
    {
        var code = "";
        var source_string = "2YU9IP1ASDFG8QWERTHJ7KLZX4CV5B3ONM6"; //自定义35进制  
        while (Id > 0)
        {
            var mod = Id % 35;
            Id = (Id - mod) / 35;
            code = source_string.ToCharArray()[mod] + code;
        }
        return code.PadRight(Length, '0'); //不足指定位补0
    }

    public static Int32 Decode(String code)
    {
        code = new String([.. (from s in code where s != '0' select s)]);
        var num = 0;
        var source_string = "2YU9IP1ASDFG8QWERTHJ7KLZX4CV5B3ONM6";
        for (var i = 0; i < code.ToCharArray().Length; i++)
        {
            for (var j = 0; j < source_string.ToCharArray().Length; j++)
            {
                if (code.ToCharArray()[i] == source_string.ToCharArray()[j])
                {
                    num += j * Convert.ToInt32(Math.Pow(35, code.ToCharArray().Length - i - 1));
                }
            }
        }
        return num;
    }

    public static String[] ShortUrl(String url)
    {
        //可以自定义生成MD5加密字符传前的混合KEY
        var key = "DengHaoNet";
        //要使用生成URL的字符
        var chars = new String[]
        {
                "a", "b", "c", "d", "e", "f", "g", "h",
                "i", "j", "k", "l", "m", "n", "o", "p",
                "q", "r", "s", "t", "u", "v", "w", "x",
                "y", "z", "0", "1", "2", "3", "4", "5",
                "6", "7", "8", "9", "A", "B", "C", "D",
                "E", "F", "G", "H", "I", "J", "K", "L",
                "M", "N", "O", "P", "Q", "R", "S", "T",
                "U", "V", "W", "X", "Y", "Z"
        };
        //对传入网址进行MD5加密
        var hex = (key + url).MD5();
        var resUrl = new String[4];
        for (var i = 0; i < 4; i++)
        {
            //把加密字符按照8位一组16进制与0x3FFFFFFF进行位与运算
            var hexint = 0x3FFFFFFF & Convert.ToInt32($"0x{hex.Substring(i * 8, 8)}", 16);
            var outChars = String.Empty;
            for (var j = 0; j < 6; j++)
            {
                //把得到的值与0x0000003D进行位与运算，取得字符数组chars索引
                var index = 0x0000003D & hexint;
                //把取得的字符相加
                outChars += chars[index];
                //每次循环按位右移5位
                hexint >>= 5;
            }
            //把字符串存入对应索引的输出数组
            resUrl[i] = outChars;
        }
        return resUrl;
    }

    private static readonly String chars = "aAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ0123456789";

    /// <summary>
    /// 转为62进制
    /// </summary>
    /// <param name="bytes">字节数组</param>
    /// <returns></returns>
    public static String ConvertTo62(Byte[] bytes)
    {
        var id = BitConverter.ToUInt32(bytes, 0);
        var list = new List<Char>();
        while (id > 0)
        {
            var item = (Int32)(id % 62);
            list.Add(chars[item]);
            id /= 62;
        }
        list.Reverse();
        return new String([.. list]);
    }

    /// <summary>
    /// 计算指定字符串的哈希值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Byte[] GetMurmurHashBytes(String str)
    {
        var hash = MurmurHash.Create32();

        var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(str));
        return bytes;
    }

    /// <summary>
    /// 获取下一个短网址Id
    /// </summary>
    /// <param name="url"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public static String GetNextCode(String url, Int64 salt)
    {
        var hashurl = url + salt;
        var bytes = GetMurmurHashBytes(hashurl);
        var code = ConvertTo62(bytes);
        return code;
    }

}