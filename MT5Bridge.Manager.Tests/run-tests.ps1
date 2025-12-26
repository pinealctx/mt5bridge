#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs MT5Bridge.MT5.Core.Tests with coverage reporting.
    运行 MT5Bridge.MT5.Core.Tests 并生成覆盖率报告。

.DESCRIPTION
    This script runs all unit tests in the MT5Bridge.MT5.Core.Tests project
    and optionally collects code coverage using coverlet.
    
    该脚本运行 MT5Bridge.MT5.Core.Tests 项目中的所有单元测试，
    并可选地使用 coverlet 收集代码覆盖率。

.PARAMETER Coverage
    Enable code coverage collection (default: false).
    启用代码覆盖率收集（默认：false）。

.PARAMETER Filter
    Filter tests by name (e.g., "DealModel").
    按名称过滤测试（例如："DealModel"）。

.PARAMETER Verbose
    Show detailed test output.
    显示详细的测试输出。

.EXAMPLE
    .\run-tests.ps1
    Run all tests without coverage.
    运行所有测试，不收集覆盖率。

.EXAMPLE
    .\run-tests.ps1 -Coverage
    Run all tests with coverage.
    运行所有测试并收集覆盖率。

.EXAMPLE
    .\run-tests.ps1 -Filter "DealModel"
    Run only tests containing "DealModel".
    仅运行包含 "DealModel" 的测试。

.EXAMPLE
    .\run-tests.ps1 -Coverage -Verbose
    Run with coverage and detailed output.
    运行并显示详细输出和覆盖率。
#>

param(
    [switch]$Coverage,
    [string]$Filter = "",
    [switch]$Verbose
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectDir = $ScriptDir
$ProjectFile = Join-Path $ProjectDir "MT5Bridge.MT5.Core.Tests.csproj"

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  MT5Bridge.MT5.Core.Tests" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Check if project file exists
if (-not (Test-Path $ProjectFile)) {
    Write-Host "❌ Error: Project file not found: $ProjectFile" -ForegroundColor Red
    exit 1
}

# Build test arguments
$testArgs = @(
    "test"
    $ProjectFile
    "--configuration", "Debug"
    "--no-build"
    "--nologo"
)

# Add filter if specified
if ($Filter) {
    $testArgs += "--filter"
    $testArgs += $Filter
    Write-Host "🔍 Filter: $Filter" -ForegroundColor Yellow
    Write-Host ""
}

# Add verbosity
if ($Verbose) {
    $testArgs += "--verbosity", "detailed"
}
else {
    $testArgs += "--verbosity", "normal"
}

# Add coverage if specified
if ($Coverage) {
    Write-Host "📊 Code coverage enabled" -ForegroundColor Green
    Write-Host ""
    
    $testArgs += "--collect:XPlat Code Coverage"
    $testArgs += "--"
    $testArgs += "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover"
}

# Run build first
Write-Host "🔨 Building project..." -ForegroundColor Yellow
dotnet build $ProjectFile --configuration Debug --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

# Run tests
Write-Host "🧪 Running tests..." -ForegroundColor Yellow
Write-Host ""

dotnet @testArgs

$testExitCode = $LASTEXITCODE

Write-Host ""

if ($testExitCode -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
}
else {
    Write-Host "❌ Some tests failed" -ForegroundColor Red
}

# Display coverage results if enabled
if ($Coverage -and $testExitCode -eq 0) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Coverage Report" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    # Find coverage file
    $coverageFiles = Get-ChildItem -Path $ProjectDir -Recurse -Filter "coverage.opencover.xml" | Select-Object -First 1
    
    if ($coverageFiles) {
        Write-Host "📊 Coverage file: $($coverageFiles.FullName)" -ForegroundColor Green
        Write-Host ""
        Write-Host "To view detailed coverage, install reportgenerator:" -ForegroundColor Yellow
        Write-Host "  dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Then generate HTML report:" -ForegroundColor Yellow
        Write-Host "  reportgenerator -reports:$($coverageFiles.FullName) -targetdir:coverage-report" -ForegroundColor Gray
        Write-Host ""
    }
    else {
        Write-Host "⚠️  Warning: Coverage file not found" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

exit $testExitCode
