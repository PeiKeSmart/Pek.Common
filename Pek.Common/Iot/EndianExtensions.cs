using System.Runtime.InteropServices;

namespace Pek.Iot;

/// <summary>
/// 数据转换
/// </summary>
public static class EndianExtensions
{
    public static Byte[] ToBytes(this Int16 v) => BitConverter.GetBytes(v);
    public static Int16 ToShort(this Byte[] v) => BitConverter.ToInt16(v, 0);
    public static UInt16 ToUShort(this Byte[] v) => BitConverter.ToUInt16(v, 0);
    public static Byte[] ToBytes(this UInt16 v) => BitConverter.GetBytes(v);
    public static Byte[] ToBytes(this Int32 v) => BitConverter.GetBytes(v);
    public static Int32 ToInt(this Byte[] v) => BitConverter.ToInt32(v, 0);
    public static Byte[] ToBytes(this UInt32 v) => BitConverter.GetBytes(v);
    public static UInt32 ToUInt(this Byte[] v) => BitConverter.ToUInt32(v, 0);
    public static Byte[] ToBytes(this Int64 v) => BitConverter.GetBytes(v);
    public static UInt64 ToULong(this Byte[] v) => BitConverter.ToUInt64(v, 0);
    public static Byte[] ToBytes(this UInt64 v) => BitConverter.GetBytes(v);
    public static Int64 ToLong(this Byte[] v) => BitConverter.ToInt64(v, 0);
    public static Int32 ToInt(this Int16 v) => v;
    public static UInt16 ToUShort(this Int16 v) => (UInt16)v;

    public static UInt32 ToUInt(this Int16 v) => (UInt32)v;
    public static Int64 ToLong(this Int16 v) => v;
    public static UInt64 ToULong(this Int16 v) => (UInt64)v;

    public static Int32 ToInt(this UInt16 v) => v;
    public static Int16 ToShort(this Int16 v) => v;
    public static UInt32 ToUInt(this UInt16 v) => v;
    public static Int64 ToLong(this UInt16 v) => v;
    public static UInt64 ToULong(this UInt16 v) => v;

    public static UInt16 ToUShort(this Int32 v) => (UInt16)v;
    public static Int16 ToShort(this Int32 v) => (Int16)v;
    public static UInt32 ToUInt(this Int32 v) => (UInt32)v;
    public static Int64 ToLong(this Int32 v) => v;
    public static UInt64 ToULong(this Int32 v) => (UInt64)v;



    public static UInt16 ToUShort(this UInt32 v) => (UInt16)v;
    public static Int16 ToShort(this UInt32 v) => (Int16)v;
    public static Int32 ToInt(this UInt32 v) => (Int32)v;
    public static Int64 ToLong(this UInt32 v) => v;
    public static UInt64 ToULong(this UInt32 v) => v;


    public static UInt16 ToUShort(this Int64 v) => (UInt16)v;
    public static Int16 ToShort(this Int64 v) => (Int16)v;
    public static Int32 ToInt(this Int64 v) => (Int32)v;
    public static UInt32 ToUInt(this Int64 v) => (UInt32)v;
    public static UInt64 ToULong(this Int64 v) => (UInt64)v;



    public static UInt16 ToUShort(this UInt64 v) => (UInt16)v;
    public static Int16 ToShort(this UInt64 v) => (Int16)v;
    public static Int32 ToInt(this UInt64 v) => (Int32)v;
    public static UInt32 ToUInt(this UInt64 v) => (UInt32)v;
    public static Int64 ToLong(this UInt64 v) => (Int64)v;


    public static Int16 Swap(this Int16 v) => (Int16)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
    public static UInt16 Swap(this UInt16 v) => (UInt16)(((v & 0xff) << 8) | ((v >> 8) & 0xff));
    public static Int32 Swap(this Int32 v)
    {
        return (Int32)(((Swap((Int16)v) & 0xffff) << 0x10) |
                      (Swap((Int16)(v >> 0x10)) & 0xffff));
    }
    public static UInt32 Swap(this UInt32 v)
    {
        return (UInt32)(((Swap((UInt16)v) & 0xffff) << 0x10) |
                       (Swap((UInt16)(v >> 0x10)) & 0xffff));
    }
    public static Int64 Swap(this Int64 v)
    {
        return (Int64)(((Swap((Int32)v) & 0xffffffffL) << 0x20) |
                       (Swap((Int32)(v >> 0x20)) & 0xffffffffL));
    }
    public static UInt64 Swap(this UInt64 v)
    {
        return (UInt64)(((Swap((UInt32)v) & 0xffffffffL) << 0x20) |
                        (Swap((UInt32)(v >> 0x20)) & 0xffffffffL));
    }

    public static Byte[] ToHexBytes(this String hexString)
    {
        hexString = hexString.Replace(" ", "");   //去除空格
        if ((hexString.Length % 2) != 0)     //判断hexstring的长度是否为偶数
        {
            hexString += "";
        }
        var returnBytes = new Byte[hexString.Length / 2];  //声明一个长度为hexstring长度一半的字节组returnBytes
        for (var i = 0; i < returnBytes.Length; i++)
        {
            returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);  //将hexstring的两个字符转换成16进制的字节组
        }
        return returnBytes;
    }

    //字节组转换成16进制的字符串：
    public static String ToHexStr(this Byte[] bytes)
    {
        var returnStr = "";
        if (bytes != null)
        {
            for (var i = 0; i < bytes.Length; i++)
            {
                returnStr += bytes[i].ToString("X2");      //byte转16进制字符
            }
        }
        return returnStr;
    }

    private static readonly UInt16[] crc_table = [0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF, 0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE, 0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D, 0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC, 0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B, 0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A, 0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49, 0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78, 0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067, 0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256, 0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405, 0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634, 0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3, 0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92, 0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1, 0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0];

    public static UInt16 ToCRC16(this Byte[] buffer) => ToShort16(buffer, (UInt32)buffer.Length, 0x00);

    public static UInt16 ToShort16(this Byte[] buffer, UInt16 precrc = 0x00) => ToShort16(buffer, (UInt32)buffer.Length, precrc);

    public static UInt16 ToShort16(this Byte[] buffer, UInt32 buffer_length, UInt16 precrc = 0x00)
    {
        var crc = precrc;
        for (UInt32 i = 0; i < buffer_length; i++)
        {
            crc = (UInt16)(crc_table[((crc >> 8) ^ buffer[i]) & 0xFF] ^ (crc << 8));
        }
        return crc;
    }

    public static Byte[] StructToBytes<T>(this T obj) where T : struct
    {
        var rawsize = Marshal.SizeOf<T>();
        var buffer = Marshal.AllocHGlobal(rawsize);
        Marshal.StructureToPtr(obj, buffer, true);
        var rawdatas = new Byte[rawsize];
        Marshal.Copy(buffer, rawdatas, 0, rawsize);
        Marshal.FreeHGlobal(buffer);
        return rawdatas;
    }

    public static T BytesToStruct<T>(this Byte[] bytes) where T : struct
    {
        T obj = default;
        var anytype = typeof(T);
        var rawsize = Marshal.SizeOf<T>();
        if (bytes != null && bytes.Length >= rawsize)
        {
            var buffer = Marshal.AllocHGlobal(rawsize);
            Marshal.Copy(bytes, 0, buffer, rawsize);
            var retobj = Marshal.PtrToStructure(buffer, anytype);
            Marshal.FreeHGlobal(buffer);
            obj = (T)retobj!;
        }
        return obj;
    }
}