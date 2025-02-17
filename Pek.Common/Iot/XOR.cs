namespace Pek.Iot;

/// <summary>
/// XOR算法
/// </summary>
public static class XOR
{
    /// <summary>
    /// 计算按位异或校验和（返回校验和值）
    /// </summary>
    /// <param name="Cmd">命令数组</param>
    /// <returns>校验和值</returns>
    public static byte GetXOR(byte[] Cmd)
    {
        byte check = (byte)(Cmd[0] ^ Cmd[1]);
        for (int i = 2; i < Cmd.Length; i++)
        {
            check = (byte)(check ^ Cmd[i]);
        }
        return check;
    }
}