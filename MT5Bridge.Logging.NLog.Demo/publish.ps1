<#
.SYNOPSIS
    MT5Bridge.Logging.NLog.Demo 发布打包脚本

.DESCRIPTION
    支持参数化打包 .NET 应用程序，可选择配置模式和自包含选项

.PARAMETER Configuration
    编译配置 (Debug 或 Release)，默认为 Release

.PARAMETER SelfContained
    是否自包含发布，默认为 false (框架依赖)

.PARAMETER Runtime
    目标运行时标识符 (RID)，例如: win-x64, linux-x64, osx-x64
    不指定则发布为框架依赖的便携版本

.PARAMETER OutputPath
    输出路径，默认为 .\publish

.PARAMETER Clean
    发布前清理输出目录

.EXAMPLE
    .\publish.ps1
    使用默认设置 (Release, 框架依赖)

.EXAMPLE
    .\publish.ps1 -Configuration Debug
    Debug 模式，框架依赖

.EXAMPLE
    .\publish.ps1 -SelfContained -Runtime win-x64
    Release 模式，自包含，Windows x64

.EXAMPLE
    .\publish.ps1 -Configuration Debug -SelfContained -Runtime win-x64 -Clean
    Debug 模式，自包含，清理后发布
#>

param(
    [Parameter(HelpMessage = "编译配置 (Debug/Release)")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(HelpMessage = "是否自包含发布")]
    [switch]$SelfContained,
    
    [Parameter(HelpMessage = "目标运行时标识符 (win-x64, linux-x64, osx-x64, etc.)")]
    [string]$Runtime = "",
    
    [Parameter(HelpMessage = "输出路径")]
    [string]$OutputPath = ".\publish",
    
    [Parameter(HelpMessage = "发布前清理输出目录")]
    [switch]$Clean
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
    Write-ColorOutput "  MT5Bridge.Logging.NLog.Demo 发布脚本" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    Write-ColorOutput "配置参数:" "Yellow"
    Write-Host "  - 配置模式: $Configuration"
    Write-Host "  - 自包含: $(if ($SelfContained) { 'Yes' } else { 'No' })"
    Write-Host "  - 运行时: $(if ($Runtime) { $Runtime } else { 'Portable (框架依赖)' })"
    Write-Host "  - 输出路径: $OutputPath"
    Write-Host "  - 清理模式: $(if ($Clean) { 'Yes' } else { 'No' })"
    Write-Host ""

    # 获取项目文件路径
    $projectFile = Join-Path $PSScriptRoot "MT5Bridge.Logging.NLog.Demo.csproj"
    
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
        "-o", $OutputPath,
        "--nologo"
    )

    # 添加自包含选项
    if ($SelfContained) {
        $publishArgs += "--self-contained", "true"
        
        # 如果是自包含，必须指定运行时
        if (-not $Runtime) {
            # 自动检测当前平台
            if ($IsWindows -or $env:OS -match "Windows") {
                $Runtime = "win-x64"
            }
            elseif ($IsLinux) {
                $Runtime = "linux-x64"
            }
            elseif ($IsMacOS) {
                $Runtime = "osx-x64"
            }
            else {
                throw "无法自动检测运行时平台，请使用 -Runtime 参数明确指定"
            }
            Write-ColorOutput "自动检测运行时: $Runtime" "Yellow"
        }
        
        $publishArgs += "-r", $Runtime
        
        # 添加单文件发布选项（可选）
        # $publishArgs += "-p:PublishSingleFile=true"
    }
    else {
        $publishArgs += "--self-contained", "false"
        
        # 如果指定了运行时但不是自包含，仍然可以发布特定运行时的框架依赖版本
        if ($Runtime) {
            $publishArgs += "-r", $Runtime
        }
    }

    # 执行发布
    Write-ColorOutput "正在发布项目..." "Yellow"
    Write-ColorOutput "命令: dotnet $($publishArgs -join ' ')" "DarkGray"
    Write-Host ""
    
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "发布失败，退出代码: $LASTEXITCODE"
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
        Write-ColorOutput "运行命令: .\$($exeFile.Name)" "Yellow"
    }
    else {
        $dllFile = Get-ChildItem -Path $OutputPath -Filter "MT5Bridge.Logging.NLog.Demo.dll" -File | Select-Object -First 1
        if ($dllFile) {
            Write-ColorOutput "DLL 文件: $($dllFile.Name)" "Green"
            Write-ColorOutput "运行命令: dotnet $($dllFile.Name)" "Yellow"
        }
    }

}
catch {
    Write-Host ""
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "✗ 发布失败" "Red"
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "错误信息: $_" "Red"
    exit 1
}
