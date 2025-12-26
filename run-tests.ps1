#!/usr/bin/env pwsh
# MT5Bridge.Core Test Runner
# Usage: .\run-tests.ps1 [-Mode <mode>] [-Filter <filter>] [-Verbosity <level>] [-NoBuild] [-Coverage]

param(
    [ValidateSet('all', 'core', 'manager', 'benchmarks', 'atomic', 'timing', 'text', 'collections', 'logging', 'filter')]
    [string]$Mode = 'all',
    
    [string]$Filter = '',
    
    [ValidateSet('quiet', 'minimal', 'normal', 'detailed')]
    [string]$Verbosity = 'minimal',
    
    [switch]$NoBuild,
    
    [switch]$Coverage
)

$ErrorActionPreference = 'Stop'

# Set UTF-8 encoding for terminal output
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Define projects
$projects = @{
    'core'       = Join-Path $scriptDir 'MT5Bridge.Core.Tests\MT5Bridge.Core.Tests.csproj'
    'manager'    = Join-Path $scriptDir 'MT5Bridge.Manager.Tests\MT5Bridge.Manager.Tests.csproj'
    'benchmarks' = Join-Path $scriptDir 'MT5Bridge.Benchmarks\MT5Bridge.Benchmarks.csproj'
}

# Determine which projects to run
$targetProjects = @()
if ($Mode -eq 'all') {
    $targetProjects = @('core', 'manager') # Benchmarks are usually run separately
}
elseif ($Mode -eq 'manager') {
    $targetProjects = @('manager')
}
elseif ($Mode -eq 'benchmarks') {
    $targetProjects = @('benchmarks')
}
elseif ($Mode -eq 'core' -or $Mode -eq 'atomic' -or $Mode -eq 'timing' -or $Mode -eq 'text' -or $Mode -eq 'collections' -or $Mode -eq 'logging' -or $Mode -eq 'filter') {
    $targetProjects = @('core')
}

# Build test filter for Core project
$testFilter = switch ($Mode) {
    'atomic' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Atomic' }
    'timing' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Timing' }
    'text' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Text' }
    'collections' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Collections' }
    'logging' { 'FullyQualifiedName~MT5Bridge.Core.Tests.Logging' }
    'filter' { $Filter }
    default { '' }
}

# Display run information
Write-Host '================================================' -ForegroundColor Cyan
Write-Host ' MT5Bridge Test Runner' -ForegroundColor Cyan
Write-Host '================================================' -ForegroundColor Cyan
Write-Host "Mode:      $Mode" -ForegroundColor Yellow
Write-Host "Projects:  $($targetProjects -join ', ')" -ForegroundColor Yellow
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

foreach ($projKey in $targetProjects) {
    $projectPath = $projects[$projKey]
    
    if (-not (Test-Path $projectPath)) {
        Write-Warning "Project not found: $projectPath"
        continue
    }

    Write-Host ">>> Running $projKey tests..." -ForegroundColor Cyan

    if ($projKey -eq 'benchmarks') {
        # Benchmarks are run as a console app in Release mode
        & dotnet run -c Release --project $projectPath
    }
    else {
        # Build test command arguments
        $testArgs = @(
            'test',
            $projectPath,
            '--logger', "console;verbosity=$Verbosity"
        )

        if ($NoBuild) {
            $testArgs += '--no-build'
        }

        if ($projKey -eq 'core' -and $testFilter) {
            $testArgs += '--filter', $testFilter
        }

        if ($Coverage) {
            $testArgs += '--collect:XPlat Code Coverage'
            $testArgs += '--results-directory', (Join-Path $scriptDir 'TestResults')
        }

        try {
            & dotnet @testArgs
        }
        catch {
            Write-Error "Test execution failed for ${projKey}: $_"
        }
    }
}

# Calculate execution time
$endTime = Get-Date
$duration = $endTime - $startTime

# Display execution time
Write-Host ''
Write-Host '================================================' -ForegroundColor Cyan
$timeMsg = "Total execution time: $($duration.TotalSeconds.ToString('F2')) seconds"
if ($LASTEXITCODE -eq 0) {
    Write-Host $timeMsg -ForegroundColor Green
}
else {
    Write-Host $timeMsg -ForegroundColor Red
}
Write-Host '================================================' -ForegroundColor Cyan

# If coverage is enabled, show results location
if ($Coverage) {
    $resultsDir = Join-Path $scriptDir 'TestResults'
    Write-Host ''
    Write-Host "Coverage reports generated at: $resultsDir" -ForegroundColor Green
}

exit $LASTEXITCODE
