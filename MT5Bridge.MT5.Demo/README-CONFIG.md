# 配置文件说明

本项目使用示例配置文件（`.example.json`）作为模板，你需要根据实际情况创建配置文件。

## 配置文件列表

| 文件                       | 说明         | 是否必需 |
| -------------------------- | ------------ | -------- |
| `appsettings.example.json` | 应用配置示例 | 模板文件 |
| `appsettings.json`         | 实际应用配置 | **必需** |
| `nlog.example.json`        | 日志配置示例 | 模板文件 |
| `nlog.json`                | 实际日志配置 | 可选     |

> **注意：** `appsettings.json` 和 `nlog.json` 已添加到 `.gitignore`，不会被提交到版本控制。

## 快速设置

### 1. 创建应用配置文件（必需）

```powershell
# Windows PowerShell
Copy-Item appsettings.example.json appsettings.json

# 或使用命令提示符
copy appsettings.example.json appsettings.json
```

然后编辑 `appsettings.json`，填入你的 MT5 连接信息：

```json
{
    "MT5Connection": {
        "Server": "your-mt5-server.com:443",
        "Login": 12345,
        "Password": "your_password",
        "TimeoutMs": 30000
    },
    "Demo": {
        "DefaultTest": "groups",
        "DefaultUserLogin": 0
    }
}
```

### 2. 创建日志配置文件（可选）

如果需要自定义日志配置：

```powershell
# Windows PowerShell
Copy-Item nlog.example.json nlog.json

# 然后编辑 nlog.json
notepad nlog.json
```

如果不创建 `nlog.json`，NLog 将使用默认配置。

## 配置文件详解

### appsettings.json

```json
{
    "MT5Connection": {
        "Server": "服务器地址:端口",     // 例如: "mt5.example.com:443"
        "Login": 管理员登录账号,          // 数字类型
        "Password": "管理员密码",         // 字符串类型
        "TimeoutMs": 30000               // 连接超时（毫秒）
    },
    "Demo": {
        "DefaultTest": "groups",         // 默认测试类型
        "DefaultUserLogin": 0            // 默认用户登录账号
    }
}
```

### nlog.json

```json
{
    "NLog": {
        "throwConfigExceptions": true,
        "targets": {
            "console": {
                "type": "Console",
                "layout": "${longdate}|${level:uppercase=true}|${logger}|${message}"
            },
            "file": {
                "type": "File",
                "fileName": "logs/mt5test-${shortdate}.log",
                "layout": "${longdate}|${level:uppercase=true}|${logger}|${message}",
                "archiveAboveSize": 10485760,  // 10MB
                "maxArchiveFiles": 10
            }
        },
        "rules": [
            {
                "logger": "*",
                "minLevel": "Debug",           // 可改为: Info, Warn, Error
                "writeTo": "console"
            },
            {
                "logger": "*",
                "minLevel": "Debug",
                "writeTo": "file"
            }
        ]
    }
}
```

## 首次运行检查清单

- [ ] 已创建 `appsettings.json`
- [ ] 已填写正确的 MT5 服务器地址
- [ ] 已填写管理员登录账号和密码
- [ ] （可选）已创建并配置 `nlog.json`
- [ ] 确认 MT5 Manager API 运行时已安装

## 运行应用

```powershell
# 查看帮助
.\MT5Bridge.MT5.Demo.exe --help

# 使用配置文件运行
.\MT5Bridge.MT5.Demo.exe --test groups

# 使用命令行参数（会覆盖配置文件）
.\MT5Bridge.MT5.Demo.exe --server "mt5.example.com:443" --login 12345 --password "pwd" --test user
```

## 自动化部署

发布脚本（`publish.ps1`）会自动处理配置文件：

1. **复制示例文件** - 始终复制 `.example.json` 文件到输出目录
2. **复制实际配置** - 如果存在实际配置文件，也会复制
3. **从示例创建** - 如果不存在实际配置文件，从示例创建（需要修改）

## 安全建议

1. ✅ **不要提交** `appsettings.json` 和 `nlog.json` 到版本控制
2. ✅ 使用强密码并定期更换
3. ✅ 在生产环境中使用专用的管理员账号
4. ✅ 定期检查日志文件，及时清理敏感信息
5. ⚠️ 部署后立即修改配置文件权限，防止未授权访问

## 故障排除

### 问题：应用无法启动
- 检查 `appsettings.json` 是否存在
- 验证 JSON 格式是否正确（使用 JSON 校验工具）

### 问题：无法连接 MT5 服务器
- 确认服务器地址格式：`hostname:port`
- 检查账号和密码是否正确
- 验证网络连接和防火墙设置

### 问题：日志未生成
- 检查是否创建了 `nlog.json`
- 确认日志目录（`logs/`）是否有写入权限
- 查看控制台输出的错误信息

## 相关文档

- [README-PUBLISH.md](README-PUBLISH.md) - 发布和部署指南
- [README.md](README.md) - 项目说明文档
