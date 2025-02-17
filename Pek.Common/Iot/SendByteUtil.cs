using NewLife.Collections;

namespace Pek.Iot;

public class SendByteUtil
{
    public static byte[] headbyte()
    {
        byte[] bs = new byte[3];
        bs[0] = 0xA5;
        bs[1] = 0xAA;
        bs[2] = 0xAC;

        return bs;
    }

    public static byte[] footbyte()
    {
        byte[] bs = new byte[3];
        bs[0] = 0xC5;
        bs[1] = 0xCC;
        bs[2] = 0xCA;
        return bs;
    }

    public static byte[] lengthbyte(params byte[][] bytes)
    {
        int? num = 0;
        foreach (byte[] item in bytes)
        {
            num += item.Length;
        }
        byte[] bs = new byte[1];
        bs[0] = (byte)num.Value;
        return bs;
    }

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
        return hexvalue.Put(true);
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
    /// <param name="commandbytes">发送时 command </param>
    /// <param name="data">发送的数据 </param>
    /// <param name="command"></param>
    public static byte[] SendByteData(byte commandbytes, byte command, byte[] data)
    {
        byte[] head = headbyte();
        byte[] commandbyteByte = commandbyte(commandbytes); //为了少执行一次 将command 放在length前面
        byte[] commandByte = commandbyte(command); //为了少执行一次 将command 放在length前面

        byte[] check;
        if (data == null)
        {
            check = checkbyte(commandbyteByte, commandByte);
        }
        else
        {
            check = checkbyte(commandbyteByte, commandByte, data);
        }
        byte[] foot = footbyte();
        if (data == null)
        {
            return totalData(head, check, commandbyteByte, commandByte, foot);
        }
        else
        {
            return totalData(head, check, commandbyteByte, commandByte, data, foot);
        }
    }

    /// <summary>
    /// 最后的数据 </summary>
    /// <param name="commandbyte">	发送时 command </param>
    /// <param name="data">	发送的数据 </param>
    /// <param name="command"></param>
    /// <returns> String </returns>
    public static string SendStringData(byte commandbyte, byte command, byte[] data)
    {
        return byteArrayToHex(SendByteData(commandbyte, command, data));
    }

}