# Pek.Common - 基类核心库

[![.NET](https://img.shields.io/badge/.NET-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-239120?style=flat&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![NuGet](https://img.shields.io/badge/NuGet-004880?style=flat&logo=nuget&logoColor=white)](https://www.nuget.org/)

Pek.Common 是一个功能丰富的 .NET 基类核心库，提供配置管理、缓存、压缩、事件处理、扩展方法等常用功能。

## ✨ 主要特性

- **🔧 配置系统** - 类型安全、AOT兼容的配置管理
- **📦 缓存系统** - 内存缓存和分布式缓存支持
- **🗜️ 压缩功能** - 多种压缩算法支持
- **⚡ 事件系统** - 轻量级事件发布订阅
- **🛠️ 扩展方法** - 丰富的扩展方法集合
- **🔐 安全功能** - 加密解密、哈希等安全工具
- **📁 虚拟文件系统** - 统一的文件访问接口

## 🚀 快速开始

### 安装

```bash
# Package Manager
Install-Package Pek.Common

# .NET CLI
dotnet add package Pek.Common

# PackageReference
<PackageReference Include="Pek.Common" Version="x.x.x" />
```

### 基本使用 - 配置系统

```csharp
using System.Text.Json.Serialization;
using Pek.Configuration;

// 1. 创建 JsonSerializerContext（AOT兼容）
[JsonSerializable(typeof(AppConfig))]
public partial class AppConfigJsonContext : JsonSerializerContext
{
}

// 2. 创建配置类
public class AppConfig : Config<AppConfig>
{
    public string? Name { get; set; } = "默认应用";
    public bool IsEnabled { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    
    // 3. 静态构造函数注册配置
    static AppConfig()
    {
        RegisterConfigForAot(AppConfigJsonContext.Default);
    }
}

// 4. 使用配置
var config = AppConfig.Current;
config.Name = "我的应用";
config.Save(); // 保存到 Config/AppConfig.config
```

## 📁 项目结构

```
Pek.Common/
├── Configuration/          # 配置系统
├── Cache/                 # 缓存功能
├── Events/                # 事件系统
├── Extensions/            # 扩展方法
├── Security/              # 安全功能
├── Helpers/               # 辅助工具
├── VirtualFileSystem/     # 虚拟文件系统
└── ...
```

## 📚 文档

- [配置系统完整指南](Doc/配置系统完整指南.md) - 详细的配置系统使用说明
- [配置系统最佳实践](Doc/配置系统最佳实践.md) - 性能优化和最佳实践
- [配置系统FAQ](Doc/配置系统FAQ.md) - 常见问题解答
- [配置系统API参考](Doc/配置系统API参考.md) - 完整的API文档和示例

## 🎯 支持的平台

- .NET 8.0+
- .NET Framework 4.6.1+
- .NET Standard 2.0+
- AOT 兼容

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📄 许可证

本项目基于 MIT 许可证开源。详见 [LICENSE](LICENSE) 文件。