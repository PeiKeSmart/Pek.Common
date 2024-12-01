using System.Collections;
using System.Net.NetworkInformation;

namespace Pek.Helpers;

public class DGNetHelper
{
    /// <summary>        
    /// 获取操作系统已用的端口号        
    /// </summary>        
    /// <returns></returns>   
    /// <remarks>来源于https://www.cnblogs.com/xdoudou/p/3605134.html</remarks>
    public static IList PortIsUsed()
    {
        //获取本地计算机的网络连接和通信统计数据的信息            
        var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();

        //返回本地计算机上的所有Tcp监听程序            
        var ipsTCP = ipGlobalProperties.GetActiveTcpListeners();

        //返回本地计算机上的所有UDP监听程序            
        var ipsUDP = ipGlobalProperties.GetActiveUdpListeners();

        //返回本地计算机上的Internet协议版本4(IPV4 传输控制协议(TCP)连接的信息。            
        var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

        var allPorts = new ArrayList();
        foreach (var ep in ipsTCP)
        {
            allPorts.Add(ep.Port);
        }

        foreach (var ep in ipsUDP)
        {
            allPorts.Add(ep.Port);
        }

        foreach (var conn in tcpConnInfoArray)
        {
            allPorts.Add(conn.LocalEndPoint.Port);
        }

        return allPorts;
    }

}