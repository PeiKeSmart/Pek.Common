# 配置文件生成测试项目

这个项目用于测试 Pek.Configuration 配置系统是否能正确生成配置文件。

## 项目结构

- `Program.cs` - 主控制台程序，演示配置文件的生成和使用
- `ConfigFileGenerationTestConsole.csproj` - 项目文件
- `README.md` - 说明文档

## 测试内容

1. **配置文件自动创建测试**
   - 检查 Config 目录和配置文件是否存在
   - 首次访问配置时是否自动创建目录和文件

2. **配置保存测试**
   - 修改配置属性
   - 调用 Save() 方法保存配置
   - 验证配置文件内容是否正确

3. **配置重新加载测试**
   - 重新加载配置
   - 验证配置是否正确从文件中读取

## 运行方法

### 方法1：使用 Visual Studio
1. 在 Visual Studio 中打开解决方案
2. 将 `ConfigFileGenerationTestConsole` 设为启动项目
3. 按 F5 运行

### 方法2：使用命令行
```bash
cd ConfigFileGenerationTestConsole
dotnet run
```

## 预期结果

运行程序后，你应该看到：

1. **目录创建**：在程序运行目录下创建 `Config` 文件夹
2. **配置文件生成**：在 `Config` 文件夹中生成 `Settings.config` 文件
3. **配置内容**：配置文件包含 JSON 格式的配置数据
4. **配置修改**：修改配置后保存，文件内容会更新
5. **配置重载**：重新加载配置时能正确读取文件内容

## 配置文件位置

配置文件会生成在以下位置：
```
[程序运行目录]/Config/Settings.config
```

例如：
```
bin/Debug/net8.0/Config/Settings.config
```

## 配置文件格式

生成的配置文件是 JSON 格式，例如：
```json
{
  "name": "测试应用配置",
  "version": "2.1.0",
  "debug": true,
  "timeoutSeconds": 90
}
```

## 故障排除

如果配置文件没有生成，请检查：

1. **权限问题**：确保程序有写入权限
2. **路径问题**：检查程序运行目录
3. **异常信息**：查看控制台输出的错误信息
4. **依赖项**：确保正确引用了 Pek.Common 项目

## 单元测试

除了控制台测试，还提供了单元测试：
- 位置：`Pek.Common.Tests/Configuration/ConfigFileGenerationTest.cs`
- 运行：使用 Visual Studio 测试资源管理器或 `dotnet test` 命令