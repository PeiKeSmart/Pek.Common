# Pek.Common 测试项目

这是 Pek.Common 库的测试项目，主要用于测试配置系统的功能。

## 项目结构

```
Pek.Common.Tests/
├── Configuration/
│   └── ConfigurationTests.cs    # 配置系统测试类
├── Program.cs                    # 测试运行器
├── Pek.Common.Tests.csproj      # 项目文件
└── README.md                     # 说明文档
```

## 运行测试

### 方法1: 使用 Visual Studio

1. 在 Visual Studio 中打开 `Pek.Common.sln` 解决方案
2. 右键点击 `Pek.Common.Tests` 项目
3. 选择 "运行测试" 或使用测试资源管理器

### 方法2: 使用命令行 (dotnet test)

```bash
# 进入测试项目目录
cd Pek.Common.Tests

# 运行所有测试
dotnet test

# 运行测试并显示详细输出
dotnet test --verbosity normal
```

### 方法3: 使用控制台程序

```bash
# 进入测试项目目录
cd Pek.Common.Tests

# 编译并运行
dotnet run
```

## 测试内容

### ConfigurationTests 类包含以下测试:

1. **TestConfigSave** - 测试配置保存功能
   - 验证配置能够正确保存到文件
   - 验证配置文件是否创建成功

2. **TestConfigReload** - 测试配置重新加载功能
   - 验证配置能够从文件正确加载
   - 验证数据一致性

3. **TestConfigFilePath** - 测试配置文件路径
   - 验证配置文件路径是否正确
   - 验证Config目录是否正确创建

4. **TestConfigPersistence** - 测试配置持久化
   - 验证配置文件内容格式
   - 验证数据是否正确写入文件

5. **TestConfigDefaults** - 测试配置默认值
   - 验证在没有配置文件时使用默认值
   - 验证默认值的正确性

6. **TestPerformance** - 性能测试
   - 测试多次访问Current属性的性能
   - 测试配置保存的性能

7. **TestConfigFileExtension** - 测试配置文件扩展名
   - 验证配置文件使用.config扩展名

8. **TestConfigDirectoryStructure** - 测试配置目录结构
   - 验证配置文件保存在Config目录
   - 验证文件名格式正确

## 配置文件位置

测试运行时，配置文件会保存在:
```
应用程序根目录/Config/Settings.config
```

## AOT 兼容性

本测试项目验证了配置系统的 AOT (Ahead-of-Time) 兼容性:
- 使用 `System.Text.Json` 的源生成器
- 避免运行时反射
- 支持原生编译

## 依赖项

- .NET 8.0
- xUnit 测试框架
- Pek.Common 库

## 注意事项

1. 测试会自动清理测试数据，不会影响实际配置
2. 每个测试都是独立的，可以单独运行
3. 测试包含性能验证，确保配置系统的高效性