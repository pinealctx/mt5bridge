<#
.SYNOPSIS
    MT5Bridge.MT5.Demo 发布打包脚本 (Windows x64)

.DESCRIPTION
    针对 Windows 64位平台的参数化打包脚本，支持 Debug/Release 和自包含选项

.PARAMETER Configuration
    编译配置 (Debug 或 Release)，默认为 Release

.PARAMETER SelfContained
    是否自包含发布，默认为 false (框架依赖)

.PARAMETER OutputPath
    输出路径，默认为 .\publish

.PARAMETER Clean
    发布前清理输出目录

.PARAMETER CopyMT5Libs
    是否复制 MT5 SDK 库文件，默认为 true

.EXAMPLE
    .\publish.ps1
    使用默认设置 (Release, 框架依赖)

.EXAMPLE
    .\publish.ps1 -Configuration Debug
    Debug 模式，框架依赖

.EXAMPLE
    .\publish.ps1 -SelfContained
    Release 模式，自包含发布

.EXAMPLE
    .\publish.ps1 -Configuration Debug -SelfContained -Clean
    Debug 模式，自包含，清理后发布
#>

param(
    [Parameter(HelpMessage = "编译配置 (Debug/Release)")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(HelpMessage = "是否自包含发布")]
    [switch]$SelfContained,
    
    [Parameter(HelpMessage = "输出路径")]
    [string]$OutputPath = ".\publish",
    
    [Parameter(HelpMessage = "发布前清理输出目录")]
    [switch]$Clean,
    
    [Parameter(HelpMessage = "是否复制 MT5 SDK 库文件")]
    [switch]$CopyMT5Libs = $true
)

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# 错误处理
$ErrorActionPreference = "Stop"

try {
    # 显示配置信息
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  MT5Bridge.MT5.Demo 发布脚本" "Cyan"
    Write-ColorOutput "  目标平台: Windows x64" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    Write-ColorOutput "配置参数:" "Yellow"
    Write-Host "  - 配置模式: $Configuration"
    Write-Host "  - 自包含: $(if ($SelfContained) { 'Yes' } else { 'No (框架依赖)' })"
    Write-Host "  - 输出路径: $OutputPath"
    Write-Host "  - 清理模式: $(if ($Clean) { 'Yes' } else { 'No' })"
    Write-Host "  - 复制MT5库: $(if ($CopyMT5Libs) { 'Yes' } else { 'No' })"
    Write-Host ""

    # 获取项目文件路径
    $projectFile = Join-Path $PSScriptRoot "MT5Bridge.MT5.Demo.csproj"
    
    if (-not (Test-Path $projectFile)) {
        throw "找不到项目文件: $projectFile"
    }

    # 清理输出目录
    if ($Clean -and (Test-Path $OutputPath)) {
        Write-ColorOutput "正在清理输出目录..." "Yellow"
        Remove-Item -Path $OutputPath -Recurse -Force
        Write-ColorOutput "✓ 清理完成" "Green"
        Write-Host ""
    }

    # 构建 dotnet publish 命令参数
    $publishArgs = @(
        "publish",
        $projectFile,
        "-c", $Configuration,
        "-r", "win-x64",
        "-o", $OutputPath,
        "--nologo"
    )

    # 添加自包含选项
    if ($SelfContained) {
        $publishArgs += "--self-contained", "true"
    }
    else {
        $publishArgs += "--self-contained", "false"
    }

    # 执行发布
    Write-ColorOutput "正在发布项目..." "Yellow"
    Write-ColorOutput "命令: dotnet $($publishArgs -join ' ')" "DarkGray"
    Write-Host ""
    
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "发布失败，退出代码: $LASTEXITCODE"
    }

    # 复制 MT5 SDK 库文件
    if ($CopyMT5Libs) {
        Write-Host ""
        Write-ColorOutput "正在复制 MT5 SDK 库文件..." "Yellow"
        
        $libsPath = Join-Path $PSScriptRoot "..\libs"
        $mt5Libs = @(
            "MetaQuotes.MT5CommonAPI64.dll",
            "MetaQuotes.MT5ManagerAPI64.dll"
        )
        
        $copiedCount = 0
        foreach ($lib in $mt5Libs) {
            $sourcePath = Join-Path $libsPath $lib
            $destPath = Join-Path $OutputPath $lib
            
            if (Test-Path $sourcePath) {
                Copy-Item -Path $sourcePath -Destination $destPath -Force
                Write-Host "  ✓ 已复制: $lib" -ForegroundColor Green
                $copiedCount++
            }
            else {
                Write-Host "  ⚠ 未找到: $lib" -ForegroundColor Yellow
            }
        }
        
        if ($copiedCount -gt 0) {
            Write-ColorOutput "✓ 已复制 $copiedCount 个 MT5 SDK 库文件" "Green"
        }
    }

    # 复制示例配置文件
    Write-Host ""
    Write-ColorOutput "正在复制示例配置文件..." "Yellow"
    
    $exampleFiles = @{
        "appsettings.example.json" = "appsettings.example.json"
        "nlog.example.json"        = "nlog.example.json"
    }
    
    foreach ($file in $exampleFiles.GetEnumerator()) {
        $sourcePath = Join-Path $PSScriptRoot $file.Key
        $destPath = Join-Path $OutputPath $file.Value
        
        if (Test-Path $sourcePath) {
            Copy-Item -Path $sourcePath -Destination $destPath -Force
            Write-Host "  ✓ 已复制: $($file.Value)" -ForegroundColor Green
        }
    }
    
    # 检查是否存在实际配置文件，如果不存在则从示例创建
    Write-Host ""
    Write-ColorOutput "正在检查配置文件..." "Yellow"
    
    $configFiles = @{
        "appsettings.json" = "appsettings.example.json"
        "nlog.json"        = "nlog.example.json"
    }
    
    foreach ($config in $configFiles.GetEnumerator()) {
        $configPath = Join-Path $PSScriptRoot $config.Key
        $examplePath = Join-Path $PSScriptRoot $config.Value
        $destConfigPath = Join-Path $OutputPath $config.Key
        
        # 如果源目录有实际配置文件，复制它
        if (Test-Path $configPath) {
            Copy-Item -Path $configPath -Destination $destConfigPath -Force
            Write-Host "  ✓ 已复制: $($config.Key) (使用实际配置)" -ForegroundColor Green
        }
        # 否则使用示例文件创建
        elseif (Test-Path $examplePath) {
            Copy-Item -Path $examplePath -Destination $destConfigPath -Force
            Write-Host "  ⚠ 已创建: $($config.Key) (从示例创建，请修改配置)" -ForegroundColor Yellow
        }
    }

    Write-Host ""
    Write-ColorOutput "========================================" "Green"
    Write-ColorOutput "✓ 发布成功!" "Green"
    Write-ColorOutput "========================================" "Green"
    Write-Host ""
    Write-ColorOutput "输出位置: $(Resolve-Path $OutputPath)" "Cyan"
    
    # 显示输出文件大小
    $outputFiles = Get-ChildItem -Path $OutputPath -File | Measure-Object -Property Length -Sum
    $totalSize = [math]::Round($outputFiles.Sum / 1MB, 2)
    Write-Host "文件数量: $($outputFiles.Count)"
    Write-Host "总大小: $totalSize MB"
    Write-Host ""
    
    # 显示可执行文件
    $exeFile = Get-ChildItem -Path $OutputPath -Filter "*.exe" -File | Select-Object -First 1
    if ($exeFile) {
        Write-ColorOutput "可执行文件: $($exeFile.Name)" "Green"
        Write-Host ""
        Write-ColorOutput "使用说明:" "Yellow"
        Write-Host "  1. 将 appsettings.example.json 复制为 appsettings.json"
        Write-Host "  2. 编辑 appsettings.json 填入 MT5 连接信息"
        Write-Host "  3. (可选) 修改 nlog.example.json 并重命名为 nlog.json"
        Write-Host "  4. 运行: .\$($exeFile.Name) --help"
    }
    else {
        $dllFile = Get-ChildItem -Path $OutputPath -Filter "MT5Bridge.MT5.Demo.dll" -File | Select-Object -First 1
        if ($dllFile) {
            Write-ColorOutput "DLL 文件: $($dllFile.Name)" "Green"
            Write-Host ""
            Write-ColorOutput "使用说明:" "Yellow"
            Write-Host "  1. 确保安装了 .NET 8.0 运行时"
            Write-Host "  2. 将 appsettings.example.json 复制为 appsettings.json"
            Write-Host "  3. 编辑 appsettings.json 填入 MT5 连接信息"
            Write-Host "  4. (可选) 修改 nlog.example.json 并重命名为 nlog.json"
            Write-Host "  5. 运行: dotnet $($dllFile.Name) --help"
        }
    }
    
    Write-Host ""
    Write-ColorOutput "重要提示:" "Yellow"
    Write-Host "  - 需要确保目标机器上有 MT5 Manager API 运行时环境"
    Write-Host "  - MT5 SDK 库文件已复制到输出目录"
    Write-Host "  - 首次运行前请配置 appsettings.json"

}
catch {
    Write-Host ""
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "✗ 发布失败" "Red"
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "错误信息: $_" "Red"
    exit 1
}
