using Pek.VirtualFileSystem;

namespace Pek.Infrastructure;

/// <summary>
/// 表示应用程序启动时配置服务和中间件的对象
/// </summary>
public interface IDHStartup
{
    /// <summary>
    /// 配置虚拟文件系统
    /// </summary>
    /// <param name="options">虚拟文件配置</param>
    void ConfigureVirtualFileSystem(DHVirtualFileSystemOptions options);

    /// <summary>
    /// 升级处理逻辑
    /// </summary>
    void Update();

    /// <summary>
    /// 处理数据
    /// </summary>
    void ProcessData();

    /// <summary>
    /// 获取此启动配置实现的顺序
    /// </summary>
    Int32 StartupOrder { get; }

    /// <summary>
    /// 获取此启动配置实现的顺序。主要针对ConfigureMiddleware、UseRouting前执行的数据、UseAuthentication或者UseAuthorization后面 Endpoints前执行的数据
    /// </summary>
    Int32 ConfigureOrder { get; }
}
