namespace Pek.Iot;

/// <summary>
/// CRC16的算法集合
/// </summary>
public static class CRC16
{
    /// <summary>
    /// CRC16_Modbus效验
    /// </summary>
    /// <param name="byteData">要进行计算的字节数组</param>
    /// <returns>计算后的数组</returns>
    public static Byte[] ToModbus(Byte[] byteData)
    {
        var CRC = new Byte[2];

        UInt16 wCrc = 0xFFFF;
        for (var i = 0; i < byteData.Length; i++)
        {
            wCrc ^= Convert.ToUInt16(byteData[i]);
            for (var j = 0; j < 8; j++)
            {
                if ((wCrc & 0x0001) == 1)
                {
                    wCrc >>= 1;
                    wCrc ^= 0xA001;//异或多项式
                }
                else
                {
                    wCrc >>= 1;
                }
            }
        }

        CRC[1] = (Byte)((wCrc & 0xFF00) >> 8);//高位在后
        CRC[0] = (Byte)(wCrc & 0x00FF);       //低位在前
        return CRC;
    }

    /// <summary>
    /// CRC16_Modbus效验
    /// </summary>
    /// <param name="byteData">要进行计算的字节数组</param>
    /// <param name="byteLength">长度</param>
    /// <returns>计算后的数组</returns>
    public static Byte[] ToModbus(Byte[] byteData, Int32 byteLength)
    {
        var CRC = new Byte[2];

        UInt16 wCrc = 0xFFFF;
        for (var i = 0; i < byteLength; i++)
        {
            wCrc ^= Convert.ToUInt16(byteData[i]);
            for (var j = 0; j < 8; j++)
            {
                if ((wCrc & 0x0001) == 1)
                {
                    wCrc >>= 1;
                    wCrc ^= 0xA001;//异或多项式
                }
                else
                {
                    wCrc >>= 1;
                }
            }
        }

        CRC[1] = (Byte)((wCrc & 0xFF00) >> 8);//高位在后
        CRC[0] = (Byte)(wCrc & 0x00FF);       //低位在前
        return CRC;
    }

    /// <summary>
    /// CRC16_LSB-MSB效验
    /// </summary>
    /// <param name="byteData">要进行计算的字节数组</param>
    /// <returns>计算后的数组</returns>
    public static Byte[] ToMsbLsb(Byte[] byteData)
    {
        var crcSwtich = new Byte[2];
        var CRC = ToModbus(byteData);

        crcSwtich[0] = CRC[1]; //高位在后
        crcSwtich[1] = CRC[0]; //低位在前

        return crcSwtich;
    }
}