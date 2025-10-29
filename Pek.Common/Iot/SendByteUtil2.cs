using NewLife.Collections;
using NewLife.Log;

namespace Pek.Iot;

/// <summary>
/// 适用于ModBus
/// </summary>
public static class SendByteUtil2
{
    public static byte[] commandbyte(byte b)
    {
        byte[] bs = new byte[1];
        bs[0] = b;
        return bs;
    }

    public static byte[] checkbyte(params byte[][] bs)
    {
        int? num = 0;
        foreach (byte[] item in bs)
        {
            num ^= getxorcheck(item);
        }
        byte[] chackbyte = new byte[1];
        chackbyte[0] = (byte)num.Value;
        return chackbyte;
    }

    public static byte getxorcheck(byte[] data)
    {
        int? num = 0;
        for (int i = 0; i < data.Length; i++)
        {
            num ^= data[i];
        }
        return (byte)num.Value;
    }

    public static byte[] totalData(params byte[][] bs)
    {
        IList<byte> bytes = new List<byte>();
        foreach (byte[] item in bs)
        {
            foreach (byte b in item)
            {
                bytes.Add(b);
            }
        }
        byte[] newbs = new byte[bytes.Count];
        for (int i = 0; i < bytes.Count; i++)
        {
            newbs[i] = bytes[i];
        }
        return newbs;
    }

    /// <summary>
    /// 将byte 转为16进制 
    /// </summary>
    public static string byteArrayToHex(byte[] bytes)
    {
        var hexvalue = Pool.StringBuilder.Get();
        for (int i = 0; i < bytes.Length; i++)
        {
            int num = bytes[i] & 0xff;
            if (num < 16)
            {
                hexvalue.Append("0");
            }
            hexvalue.Append(num.ToString("x"));
        }
        return hexvalue.ToString();
    }

    /// <summary>
    /// 数组截取 
    /// </summary>
    public static byte[] subbyteArray(byte[] bs, int start, int end)
    {
        if (end <= start)
        {
            return null;
        }
        byte[] ary = new byte[end - start];
        Array.Copy(bs, start, ary, 0, end - start);
        return ary;
    }

    /// <summary>
    /// 最后的数据 </summary>
    /// <param name="commandb">设备地址</param>
    /// <param name="data">发送的数据 </param>
    /// <param name="commandbyte1"></param>
    public static byte[] SendByteData(byte commandb, byte commandbyte1, byte[] data)
    {
        byte[] command = commandbyte(commandb);    // 设备地址
        byte[] command1 = commandbyte(commandbyte1);  // 功能码

        var Crc16res = CRC16(totalData(command, command1, data));
        byte[] produceCRC = new byte[2];
        produceCRC[0] = (byte)Crc16res;
        produceCRC[1] = (byte)((Convert.ToInt32(Crc16res)) / 256); //将校验码添加进命令帧

        return totalData(command, command1, data, produceCRC);
    }

    /// <summary>
    /// 最后的数据 
    /// </summary>
    /// <param name="commandbyte">	发送时 command </param>
    /// <param name="data">	发送的数据 </param>
    /// <param name="command"></param>
    /// <returns> String </returns>
    public static string SendStringData(byte commandbyte, byte command, byte[] data)
    {
        var cmd = byteArrayToHex(SendByteData(commandbyte, command, data));

        XTrace.WriteLine($"下发的指令：{cmd}");

        return cmd;
    }

    /// <summary>
    /// 最后的数据 
    /// </summary>
    /// <param name="commandbyte">	发送时 command </param>
    /// <param name="data">	发送的数据 </param>
    /// <param name="command"></param>
    /// <returns> String </returns>
    public static string SendStringData(byte commandbyte, byte command, byte[] data, String Rnd)
    {
        var cmd = Rnd.PadLeft(4, '0') + byteArrayToHex(SendByteData(commandbyte, command, data));

        XTrace.WriteLine($"下发的指令：{cmd}");

        return cmd;
    }

    /// <summary>
    /// 累加校验和
    /// </summary>
    /// <param name="memorySpage">需要计算校验和的byte数组</param>
    /// <param name="Length">校验和位数</param>
    /// <returns>计算出的校验和数组</returns>
    public static Byte[] CheckSum(Byte[] memorySpage, Int32 Length)
    {
        var mSum = 0L;
        var mByte = new Byte[Length];

        // 逐Byte添加位数和
        foreach (var byteMsg in memorySpage)
        {
            Int64 mNum = byteMsg >= 0 ? byteMsg : byteMsg + 256;
            mSum += mNum;
        }

        // 位数和转化为Byte数组
        for (var liv_Count = 0; liv_Count < Length; liv_Count++)
        {
            mByte[Length - liv_Count - 1] = (Byte)(mSum >> (liv_Count * 8) & 0xff);
        }

        return mByte;
    }

    /// <summary>
    /// CRC16检验
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static ushort CRC16(byte[] bytes)
    {
        ushort value;
        ushort newLoad = 0xffff, In_value;
        int count = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            value = (ushort)bytes[i];
            newLoad = (ushort)(Convert.ToInt32(value) ^ Convert.ToInt32(newLoad));
            In_value = 0xA001;
            while (count < 8)
            {
                if (Convert.ToInt32(newLoad) % 2 == 1)//判断最低位是否为1
                {
                    newLoad -= 0x00001;
                    newLoad = (ushort)(Convert.ToInt32(newLoad) / 2);//右移一位
                    count++;//计数器加一
                    newLoad = (ushort)(Convert.ToInt32(newLoad) ^ Convert.ToInt32(In_value));//异或操作
                }
                else
                {
                    newLoad = (ushort)(Convert.ToInt32(newLoad) / 2);//右移一位
                    count++;//计数器加一
                }
            }
            count = 0;
        }
        return newLoad;
    }

    /// <summary>
    /// S8转换
    /// </summary>
    /// <param name="Num"></param>
    /// <returns></returns>
    public static Int32 BackS8(Int32 Num)
    {
        if (Num >= 128)
            Num = -(256 - Num);

        return Num;
    }

    /// <summary>
    /// S16转换
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static Int32 BackS16(String str)
    {
        if (Convert.ToInt32(str, 16) == 0)
        {
            return 0;
        }

        return unchecked((short)Convert.ToUInt16(str, 16));
    }

    /// <summary>
    /// 将byte转为Int16数值
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static Int16 ByteToInt16(Byte src)
    {
        Int16 value;
        value = (Int16)(src & 0xFF);

        return value;
    }

    /// <summary>
    /// 将byte转为Int16数值 特殊处理，255返回-1
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static Int16 Byte1ToInt16(Byte src)
    {
        Int16 value;
        value = (Int16)(src & 0xFF);

        if (value == 255) return -1;

        return value;
    }

    /// <summary>
    /// 将Int16数值转换为占四个字节的byte数组，本方法适用于(高位在前，低位在后)的顺序
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] Int16ToBytes2(Int16 value)
    {
        byte[] src = new byte[2];
        src[0] = (byte)((value >> 8) & 0xFF);
        src[1] = (byte)(value & 0xFF);
        return src;
    }

    /// <summary>
    /// byte数组中取Int16数值，本方法适用于(低位在后，高位在前)的顺序 等同于D0*256 + D1
    /// </summary>
    /// <param name="src"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Int16 BytesToInt16(byte[] src, int offset = 0)
    {
        Int16 value;
        value = (Int16)(((src[offset] & 0xFF) << 8)
                | (src[offset + 1] & 0xFF));
        return value;
    }

    /// <summary>
    /// byte数组中取UInt16数值，本方法适用于(低位在后，高位在前)的顺序 等同于D0*256 + D1
    /// </summary>
    /// <param name="src"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static UInt16 BytesToUInt16(byte[] src, int offset = 0)
    {
        UInt16 value;
        value = (UInt16)(((src[offset] & 0xFF) << 8)
                | (src[offset + 1] & 0xFF));
        return value;
    }

    /// <summary>
    /// 适用于低位在前，高位在后
    /// </summary>
    /// <param name="value">要转换的int值</param>
    /// <returns>byte数组</returns>
    public static byte[] intToBytes(int value)
    {
        byte[] src = new byte[4];
        src[3] = (byte)((value >> 24) & 0xFF);
        src[2] = (byte)((value >> 16) & 0xFF);
        src[1] = (byte)((value >> 8) & 0xFF);
        src[0] = (byte)(value & 0xFF);
        return src;
    }

    /// <summary>
    /// 将int数值转换为占四个字节的byte数组，本方法适用于(高位在前，低位在后)的顺序
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] intToBytes2(int value)
    {
        byte[] src = new byte[4];
        src[0] = (byte)((value >> 24) & 0xFF);
        src[1] = (byte)((value >> 16) & 0xFF);
        src[2] = (byte)((value >> 8) & 0xFF);
        src[3] = (byte)(value & 0xFF);
        return src;
    }

    /// <summary>
    /// byte数组中取int数值，本方法适用于(低位在前，高位在后)的顺序，和和intToBytes（）配套使用
    /// </summary>
    /// <param name="src"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static int bytesToInt(byte[] src, int offset)
    {
        int value;
        value = (int)((src[offset] & 0xFF)
                | ((src[offset + 1] & 0xFF) << 8)
                | ((src[offset + 2] & 0xFF) << 16)
                | ((src[offset + 3] & 0xFF) << 24));
        return value;
    }

    /// <summary>
    /// byte数组中取int数值，本方法适用于(低位在后，高位在前)的顺序。和intToBytes2（）配套使用
    /// </summary>
    /// <param name="src"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static int bytesToInt2(byte[] src, int offset)
    {
        int value;
        value = (int)(((src[offset] & 0xFF) << 24)
                | ((src[offset + 1] & 0xFF) << 16)
                | ((src[offset + 2] & 0xFF) << 8)
                | (src[offset + 3] & 0xFF));
        return value;
    }

    /// <summary>
    /// byte数组中取int数值，本方法适用于每两个字节(低位在前，高位在后)的顺序，和和intToBytes3（）配套使用
    /// </summary>
    /// <param name="src"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static int bytesToInt3(byte[] src, int offset = 0)
    {
        int value;
        value = (int)(((src[offset + 2] & 0xFF) << 24)
                | ((src[offset + 3] & 0xFF) << 16)
                | ((src[offset] & 0xFF) << 8)
                | (src[offset + 1] & 0xFF));
        return value;
    }

    /// <summary>
    /// 取byte的高4位
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static int getHeight4(byte data)
    {
        //获取高四位
        int height;
        height = ((data & 0xf0) >> 4);
        return height;
    }

    /// <summary>
    /// 取byte的低4位
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static int getLow4(byte data)
    {
        //获取低四位
        int low;
        low = (data & 0x0f);
        return low;
    }

    /// <summary>
    /// 获取CHKSUM
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static Int32 GetASCIISum(this String input)
    {
        var sum = 0;

        foreach (var item in input)
        {
            sum += HexConv.HexToDec(item.ToHex()).ToInt();
        }
        var result = sum % 65535;   // 取模
        var result1 = 65535 - result + 1;  // 取反

        return result1;
    }

    /// <summary>
    /// 普通字符串转16进制ASCII码 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static String GetASCIIHex(this String input)
    {
        var content = String.Empty;

        foreach (var item in input)
        {
            content += item.ToHex();
        }

        return content;
    }

    /// <summary>
    /// 将16进制ASCII码转字符串
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string GetASCIItoStr(this String str)
    {
        byte[] bb = Hex2Bytes(str, false);
        if (bb == null || !bb.Any()) return String.Empty;

        System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
        string strCharacter = asciiEncoding.GetString(bb);
        return strCharacter;
    }

    /// <summary>
    /// 将16进制ASCII码转字节数组
    /// </summary>
    /// <param name="sHex"></param>
    /// <param name="isExchange"></param>
    /// <returns></returns>
    public static byte[] Hex2Bytes(this String sHex, bool isExchange)
    {
        if (sHex == null || sHex.Length == 0)
            return null;
        sHex = sHex.Length % 2 == 0 ? sHex : "0" + sHex;
        byte[] bRtns = new byte[sHex.Length / 2];
        for (int i = 0; i < bRtns.Length; i++)
        {
            if (isExchange)
                bRtns[bRtns.Length - 1 - i] = Convert.ToByte(sHex.Substring(i * 2, 2), 16);
            else
                bRtns[i] = Convert.ToByte(sHex.Substring(i * 2, 2), 16);
        }
        return bRtns;
    }

}