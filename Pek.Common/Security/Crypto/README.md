# 密码可逆加密系统 (PasswordCrypto)

一个高性能、支持密钥轮换的密码可逆加密系统，基于 AES-256-GCM 和 PBKDF2 算法。

> **重要提示：此系统仅支持 .NET 6.0 及以上版本**

## 目录

- [特性与规格](#特性与规格)
- [核心机制](#核心机制)
- [使用示例](#使用示例)
- [配置说明](#配置说明)
- [安全特性](#安全特性)
- [最佳实践](#最佳实践)
- [常见问题](#常见问题)

## 特性与规格

### 主要特性

- **AES-256-GCM 加密**: 高安全性认证加密，防篡改
- **PBKDF2-SHA256 密钥派生**: 100,000 次迭代，防彩虹表攻击
- **智能密钥选择**: 随机选择主密钥，支持负载均衡
- **密钥轮换支持**: 通过 KeyId 管理多版本密钥
- **批量操作**: 支持批量加密，提高吞吐量
- **完善异常处理**: 详细的错误信息和异常类型

### 技术规格

- **密文格式**: `Version(1) | KeyId(1) | Nonce(12) | 密文 | Tag(16)` => Base64
- **密钥长度**: 256位 AES 密钥
- **随机数**: 12字节 Nonce（GCM标准）
- **认证标签**: 16字节（GCM标准）
- **主密钥池**: 支持最多255个主密钥

## 核心机制

### Salt 机制

**作用**: Salt 确保相同密码产生不同密文，防彩虹表攻击并实现用户隔离。

**工作原理**:

```csharp
// Salt + KeyId 混合生成唯一派生密钥
var saltBytes = Encoding.UTF8.GetBytes(salt + keyId);
var derivedKey = new Rfc2898DeriveBytes(masterKey, saltBytes, 100000, HashAlgorithmName.SHA256);
```

**推荐格式**:

```csharp
var salt = $"user_{userId}_app_{appName}";     // 基础格式
var salt = $"tenant_{tenantId}_user_{userId}"; // 多租户格式
```

### 主密钥池机制

**作用**: 支持多个主密钥并存，实现密钥轮换、负载均衡和灾难恢复。

**工作原理**:

1. 随机选择密钥ID（或指定）
2. 使用对应主密钥 + Salt + KeyId 派生加密密钥
3. KeyId 写入密文头部，解密时自动选择正确主密钥

**推荐配置**: 5-10个密钥，使用密码学安全的随机生成

## 使用示例

```csharp
// 基本加密解密
var encrypted = PasswordCrypto.Encrypt("myPassword", "user_123_app_main");
var decrypted = PasswordCrypto.Decrypt(encrypted, "user_123_app_main");

// 指定密钥版本
var encrypted = PasswordCrypto.Encrypt("myPassword", "user_123_app_main", keyId: 2);

// 批量加密
var batchData = new List<(string, string)> { ("pwd1", "salt1"), ("pwd2", "salt2") };
var results = PasswordCrypto.EncryptBatch(batchData);

// 密钥轮换
var newEncrypted = PasswordCrypto.ReEncrypt(oldEncrypted, "user_123_app_main", newKeyId: 1);
```

## 配置说明

```csharp
[Config("PasswordCrypto")]
public class PasswordCryptoSetting : Config<PasswordCryptoSetting>
{
    /// <summary>主密钥池（支持密钥轮换）</summary>
    public String[] PasswordKeys { get; set; } = [
        Randoms.RandomStr(32), // KeyId: 0
        Randoms.RandomStr(32), // KeyId: 1
        // ... 推荐5-10个
    ];
    
    /// <summary>PBKDF2迭代次数（默认100000）</summary>
    public int Pbkdf2Iterations { get; set; } = 100000;
}
```

## 安全特性

1. **认证加密**: AES-GCM 模式提供加密和认证，防止篡改
2. **多重密钥派生**: PBKDF2-SHA256 + Salt + KeyId 三重混合
3. **用户隔离**: 每个用户的 Salt 确保密码互不干扰
4. **防彩虹表**: 唯一 Salt 使预计算攻击无效
5. **密钥轮换**: 支持平滑的密钥升级和版本管理
6. **随机性保证**: 使用密码学安全的随机数生成器

## 最佳实践

### Salt 设计

```csharp
// ✅ 推荐
var salt = $"user_{userId}_app_{appName}";
var salt = $"tenant_{tenantId}_user_{userId}";

// ❌ 避免
var salt = "fixed_salt";           // 固定值
var salt = userId.ToString();      // 过于简单
```

### 密钥管理

```csharp
// ✅ 推荐：随机密钥选择
var encrypted = PasswordCrypto.Encrypt(password, salt);

// ✅ 推荐：定期轮换
var reEncrypted = PasswordCrypto.ReEncrypt(oldEncrypted, salt);

// ❌ 避免：循环单个加密（性能差）
foreach (var item in items)
    var encrypted = PasswordCrypto.Encrypt(item.Password, item.Salt);
```

### 异常处理

```csharp
try
{
    var decrypted = PasswordCrypto.Decrypt(encrypted, salt);
}
catch (UnauthorizedAccessException)
{
    // 解密失败，可能需要密钥轮换
}
catch (ArgumentException ex)
{
    // 参数错误（密文格式、Salt为空等）
}
```

## 常见问题

**Q: 支持哪些.NET版本？**  
A: 仅支持 .NET 6.0+，低版本会跳过编译。

**Q: Salt需要保密吗？**  
A: 不需要，Salt确保唯一性而非保密性。

**Q: 主密钥池最佳大小？**  
A: 推荐5-10个密钥，平衡安全性和可管理性。

**Q: 如何处理密钥泄露？**  
A: 添加新密钥 → 批量重新加密 → 移除泄露密钥

**Q: 密文可以跨环境迁移吗？**  
A: 可以，但需要相同的主密钥池配置。

---

**性能**: 单次加密~13.7ms，1000次批量~13.7秒  
**错误类型**: ArgumentException, InvalidOperationException, UnauthorizedAccessException, FormatException
