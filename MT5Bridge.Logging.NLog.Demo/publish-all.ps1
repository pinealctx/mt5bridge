<#
.SYNOPSIS
    批量发布多个平台的版本

.DESCRIPTION
    一键发布多个配置和平台的版本，适合制作发行包

.PARAMETER Configuration
    编译配置 (Debug 或 Release)，默认为 Release

.PARAMETER Platforms
    要发布的平台列表，默认为 win-x64, linux-x64, osx-x64

.EXAMPLE
    .\publish-all.ps1
    发布所有平台的 Release 版本（自包含）

.EXAMPLE
    .\publish-all.ps1 -Configuration Debug -Platforms win-x64,linux-x64
    发布 Windows 和 Linux 的 Debug 版本
#>

param(
    [Parameter(HelpMessage = "编译配置 (Debug/Release)")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(HelpMessage = "目标平台列表")]
    [string[]]$Platforms = @("win-x64", "linux-x64", "osx-x64")
)

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

$ErrorActionPreference = "Stop"

try {
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  批量发布所有平台版本" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    Write-Host "配置: $Configuration"
    Write-Host "平台: $($Platforms -join ', ')"
    Write-Host ""
    
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $baseOutputPath = ".\publish-all\$Configuration-$timestamp"
    
    $successCount = 0
    $failCount = 0
    $results = @()
    
    foreach ($platform in $Platforms) {
        Write-ColorOutput "----------------------------------------" "Yellow"
        Write-ColorOutput "正在发布平台: $platform" "Yellow"
        Write-ColorOutput "----------------------------------------" "Yellow"
        Write-Host ""
        
        $outputPath = Join-Path $baseOutputPath $platform
        
        try {
            # 调用单个发布脚本
            & "$PSScriptRoot\publish.ps1" `
                -Configuration $Configuration `
                -SelfContained `
                -Runtime $platform `
                -OutputPath $outputPath `
                -Clean
            
            $successCount++
            $results += [PSCustomObject]@{
                Platform = $platform
                Status = "✓ 成功"
                Path = $outputPath
            }
            
            Write-Host ""
        } catch {
            $failCount++
            $results += [PSCustomObject]@{
                Platform = $platform
                Status = "✗ 失败"
                Path = $_.Exception.Message
            }
            
            Write-ColorOutput "平台 $platform 发布失败: $_" "Red"
            Write-Host ""
        }
    }
    
    # 显示汇总结果
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  发布汇总" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    
    $results | Format-Table -AutoSize
    
    Write-Host ""
    Write-Host "成功: $successCount"
    Write-Host "失败: $failCount"
    Write-Host ""
    
    if ($successCount -gt 0) {
        Write-ColorOutput "所有输出位置: $(Resolve-Path $baseOutputPath)" "Green"
        
        # 创建压缩包
        $zipPath = "$baseOutputPath.zip"
        Write-ColorOutput "正在创建压缩包..." "Yellow"
        
        if (Test-Path $zipPath) {
            Remove-Item $zipPath -Force
        }
        
        Compress-Archive -Path $baseOutputPath\* -DestinationPath $zipPath
        
        $zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
        Write-ColorOutput "✓ 压缩包已创建: $zipPath ($zipSize MB)" "Green"
    }
    
    Write-Host ""
    
    if ($failCount -eq 0) {
        Write-ColorOutput "✓ 所有平台发布成功!" "Green"
    } else {
        Write-ColorOutput "⚠ 部分平台发布失败，请检查错误信息" "Yellow"
        exit 1
    }
    
} catch {
    Write-ColorOutput "批量发布失败: $_" "Red"
    exit 1
}
