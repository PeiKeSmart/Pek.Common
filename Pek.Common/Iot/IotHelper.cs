﻿namespace Pek.Iot;

public class IotHelper
{
    /// <summary>
    /// 把int32类型的数据转存到4个字节的byte数组中
    /// </summary>
    /// <param name="m">int32类型的数据</param>
    /// <param name="arry">4个字节大小的byte数组</param>
    /// <returns></returns>
    public static Boolean ConvertIntToByteArray(Int32 m, ref Byte[] arry)
    {
        if (arry == null) return false;
        if (arry.Length < 4) return false;

        arry[0] = (Byte)(m & 0xFF);
        arry[1] = (Byte)((m & 0xFF00) >> 8);
        arry[2] = (Byte)((m & 0xFF0000) >> 16);
        arry[3] = (Byte)(m >> 24 & 0xFF);

        return true;
    }
}
