# 配置文件自动重新加载测试

这个控制台应用程序用于测试配置系统的自动重新加载功能。

## 功能特性

- **自动监控配置文件变更**：当配置文件被修改时，系统会自动重新加载配置
- **实时配置更新**：内存中的配置对象会自动同步更新
- **配置变更事件**：提供配置变更事件通知
- **中文字符支持**：配置文件中的中文字符正确显示

## 测试步骤

1. **运行程序**：
   ```bash
   dotnet run
   ```

2. **观察初始配置**：
   - 程序会显示当前配置值
   - 如果配置文件不存在，会自动创建

3. **手动修改配置文件**：
   - 配置文件位置：`bin/Debug/net8.0/Config/TestSettings.config`
   - 修改任意配置值（如name、version、debug、timeoutSeconds）
   - 保存文件

4. **观察自动重新加载**：
   - 程序会检测到文件变更
   - 自动重新加载配置
   - 触发配置变更事件
   - 显示新的配置值

## 配置文件示例

```json
{
  "name": "测试应用配置",
  "version": "1.0.0",
  "debug": true,
  "timeoutSeconds": 30
}
```

## 实现原理

1. **文件监控**：使用`FileWatcher`类监控Config目录下的文件变更
2. **自动重新加载**：检测到.config文件变更时，自动重新加载对应的配置类型
3. **内存同步**：通过配置变更事件，自动更新内存中的配置实例
4. **延迟处理**：文件变更后延迟100ms再读取，避免文件正在写入时的读取冲突

## API 使用

### 启用/禁用自动重新加载

```csharp
// 启用自动重新加载（默认启用）
ConfigManager.SetAutoReload(true);

// 禁用自动重新加载
ConfigManager.SetAutoReload(false);
```

### 订阅配置变更事件

```csharp
ConfigManager.ConfigChanged += (configType, newConfig) =>
{
    Console.WriteLine($"配置已更新: {configType.Name}");
    // 处理配置变更逻辑
};
```

### 获取当前配置

```csharp
// 获取当前配置实例（自动同步）
var config = TestSettings.Current;
```

## 注意事项

- 自动重新加载功能默认启用
- 只监控.config文件的变更事件
- 配置变更后会自动更新`Config<T>.Current`属性
- 文件监控器会在应用程序退出时自动停止