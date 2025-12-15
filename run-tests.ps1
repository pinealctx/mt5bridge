#!/usr/bin/env pwsh
# MT5Bridge.Core Test Runner
# Usage: .\run-tests.ps1 [-Mode <mode>] [-Filter <filter>] [-Verbosity <level>] [-NoBuild] [-Coverage]

param(
    [ValidateSet('all', 'atomic', 'timing', 'text', 'collections', 'logging', 'filter')]
    [string]$Mode = 'all',
    
    [string]$Filter = '',
    
    [ValidateSet('quiet', 'minimal', 'normal', 'detailed')]
    [string]$Verbosity = 'minimal',
    
    [switch]$NoBuild,
    
    [switch]$Coverage
)

$ErrorActionPreference = 'Stop'

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$testProject = Join-Path $scriptDir 'MT5Bridge.Core.Tests\MT5Bridge.Core.Tests.csproj'

# Check if test project exists
if (-not (Test-Path $testProject)) {
    Write-Error "Test project not found: $testProject"
    exit 1
}

# Build test filter
$testFilter = switch ($Mode) {
    'atomic' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Atomic' }
    'timing' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Timing' }
    'text' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Text' }
    'collections' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Collections' }
    'logging' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Logging' }
    'filter' { $Filter }
    default { '' }
}

# Build test command arguments (using array for reliable parameter passing)
$testArgs = @(
    'test',
    $testProject,
    '--logger', "console;verbosity=$Verbosity"
)

if ($NoBuild) {
    $testArgs += '--no-build'
}

if ($testFilter) {
    $testArgs += '--filter', $testFilter
}

if ($Coverage) {
    $testArgs += '--collect:XPlat Code Coverage'
    $testArgs += '--results-directory', (Join-Path $scriptDir 'TestResults')
}

# Display run information
Write-Host '================================================' -ForegroundColor Cyan
Write-Host ' MT5Bridge.Core Test Runner' -ForegroundColor Cyan
Write-Host '================================================' -ForegroundColor Cyan
Write-Host "Mode:      $Mode" -ForegroundColor Yellow
if ($testFilter) {
    Write-Host "Filter:    $testFilter" -ForegroundColor Yellow
}
Write-Host "Verbosity: $Verbosity" -ForegroundColor Yellow
if ($Coverage) {
    Write-Host "Coverage:  Enabled" -ForegroundColor Yellow
}
Write-Host '================================================' -ForegroundColor Cyan
Write-Host ''

# Record start time
$startTime = Get-Date

# Run tests
try {
    & dotnet @testArgs
    $exitCode = $LASTEXITCODE
}
catch {
    Write-Error "Test execution failed: $_"
    exit 1
}

# Calculate execution time
$endTime = Get-Date
$duration = $endTime - $startTime

# Display execution time
Write-Host ''
Write-Host '================================================' -ForegroundColor Cyan
$timeMsg = "Test execution time: $($duration.TotalSeconds.ToString('F2')) seconds"
if ($exitCode -eq 0) {
    Write-Host $timeMsg -ForegroundColor Green
}
else {
    Write-Host $timeMsg -ForegroundColor Red
}
Write-Host '================================================' -ForegroundColor Cyan

# If coverage is enabled, show results location
if ($Coverage -and $exitCode -eq 0) {
    $resultsDir = Join-Path $scriptDir 'TestResults'
    Write-Host ''
    Write-Host "Coverage report generated at: $resultsDir" -ForegroundColor Green
    Write-Host 'Tip: Use ReportGenerator tool to generate HTML report:' -ForegroundColor Yellow
    Write-Host '  dotnet tool install -g dotnet-reportgenerator-globaltool' -ForegroundColor Gray
    Write-Host '  reportgenerator -reports:TestResults\**\coverage.cobertura.xml \' -ForegroundColor Gray
    Write-Host '                  -targetdir:TestResults\html -reporttypes:Html' -ForegroundColor Gray
}

exit $exitCode
