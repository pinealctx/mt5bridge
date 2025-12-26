<#
.SYNOPSIS
    MT5Bridge.Serilog.Demo Release Script for Windows

.DESCRIPTION
    Packaging script for publishing the Serilog logging demonstration application.
    Supports Debug/Release configurations and self-contained options.

.PARAMETER Configuration
    Build configuration (Debug or Release), default is Release

.PARAMETER SelfContained
    Whether to publish as self-contained, default is false (framework-dependent)

.PARAMETER OutputPath
    Output path, default is .\publish

.PARAMETER Clean
    Clean output directory before publishing

.PARAMETER IncludeSymbols
    Include debug symbols (PDB files) in output, default is false

.PARAMETER SingleFile
    Publish as a single executable file (requires SelfContained)

.EXAMPLE
    .\publish.ps1
    Use default settings (Release, framework-dependent, no debug symbols)

.EXAMPLE
    .\publish.ps1 -Configuration Debug
    Debug mode, framework-dependent

.EXAMPLE
    .\publish.ps1 -SelfContained
    Release mode, self-contained (multiple DLLs)

.EXAMPLE
    .\publish.ps1 -SelfContained -SingleFile
    Release mode, self-contained, single EXE file

.EXAMPLE
    .\publish.ps1 -IncludeSymbols
    Release mode with debug symbols for troubleshooting

.EXAMPLE
    .\publish.ps1 -Configuration Debug -SelfContained -Clean
    Debug mode, self-contained, clean build
#>

param(
    [Parameter(HelpMessage = "Build configuration (Debug/Release)")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(HelpMessage = "Publish as self-contained")]
    [switch]$SelfContained,
    
    [Parameter(HelpMessage = "Output path")]
    [string]$OutputPath = ".\publish",
    
    [Parameter(HelpMessage = "Clean output directory before publishing")]
    [switch]$Clean,
    
    [Parameter(HelpMessage = "Include debug symbols (PDB files)")]
    [switch]$IncludeSymbols,
    
    [Parameter(HelpMessage = "Publish as single executable file")]
    [switch]$SingleFile
)

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

$ErrorActionPreference = "Stop"

try {
    # Validate parameters
    if ($SingleFile -and -not $SelfContained) {
        Write-ColorOutput "Error: -SingleFile requires -SelfContained" "Red"
        Write-Host "Usage: .\publish.ps1 -SelfContained -SingleFile"
        exit 1
    }
    
    Write-ColorOutput "========================================" "Cyan"
    Write-ColorOutput "  MT5Bridge.Serilog.Demo Build Script" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""

    Write-ColorOutput "Build Parameters:" "Yellow"
    Write-Host "  - Configuration: $Configuration"
    Write-Host "  - Self-Contained: $(if ($SelfContained) { 'Yes' } else { 'No (Framework-Dependent)' })"
    Write-Host "  - Single File: $(if ($SingleFile) { 'Yes' } else { 'No' })"
    Write-Host "  - Output Path: $OutputPath"
    Write-Host "  - Clean Mode: $(if ($Clean) { 'Yes' } else { 'No' })"
    Write-Host "  - Include Symbols: $(if ($IncludeSymbols) { 'Yes (PDB files)' } else { 'No' })"
    Write-Host ""

    $projectFile = Join-Path $PSScriptRoot "MT5Bridge.Serilog.Demo.csproj"
    
    if (-not (Test-Path $projectFile)) {
        throw "Project file not found: $projectFile"
    }

    if ($Clean -and (Test-Path $OutputPath)) {
        Write-ColorOutput "Cleaning output directory..." "Yellow"
        Remove-Item -Path $OutputPath -Recurse -Force
        Write-ColorOutput "Done cleaning" "Green"
        Write-Host ""
    }

    $publishArgs = @(
        "publish",
        $projectFile,
        "-c", $Configuration,
        "-o", $OutputPath,
        "--nologo"
    )

    if ($SelfContained) {
        $publishArgs += "--self-contained", "true"
    }
    else {
        $publishArgs += "--self-contained", "false"
    }
    
    # Single file publishing
    if ($SingleFile) {
        $publishArgs += "-p:PublishSingleFile=true"
        $publishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
    }
    
    # Control debug symbols output
    if (-not $IncludeSymbols) {
        $publishArgs += "-p:DebugType=none", "-p:DebugSymbols=false"
    }

    Write-ColorOutput "Publishing project..." "Yellow"
    Write-ColorOutput "Command: dotnet $($publishArgs -join ' ')" "DarkGray"
    Write-Host ""
    
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed with exit code: $LASTEXITCODE"
    }

    Write-Host ""
    Write-ColorOutput "Copying configuration files..." "Yellow"
    
    # Check if actual config files exist
    $hasActualConfig = (Test-Path (Join-Path $PSScriptRoot "appsettings.json"))
    
    if ($hasActualConfig) {
        # If actual config exists, copy it
        $configFile = "appsettings.json"
        $sourcePath = Join-Path $PSScriptRoot $configFile
        $destPath = Join-Path $OutputPath $configFile
        
        Copy-Item -Path $sourcePath -Destination $destPath -Force
        Write-Host "  Copied: $configFile" -ForegroundColor Green
        
        Write-Host ""
        Write-ColorOutput "Using actual config file from project directory" "Cyan"
    }

    Write-Host ""
    Write-ColorOutput "========================================" "Green"
    Write-ColorOutput "Build Successful!" "Green"
    Write-ColorOutput "========================================" "Green"
    Write-Host ""
    Write-ColorOutput "Output Location: $(Resolve-Path $OutputPath)" "Cyan"
    
    $outputFiles = Get-ChildItem -Path $OutputPath -File | Measure-Object -Property Length -Sum
    $totalSize = [math]::Round($outputFiles.Sum / 1MB, 2)
    Write-Host "File Count: $($outputFiles.Count)"
    Write-Host "Total Size: $totalSize MB"
    Write-Host ""
    
    $exeFile = Get-ChildItem -Path $OutputPath -Filter "*.exe" -File | Select-Object -First 1
    if ($exeFile) {
        Write-ColorOutput "Executable: $($exeFile.Name)" "Green"
        Write-Host ""
        Write-ColorOutput "Usage Instructions:" "Yellow"
        Write-Host "  1. Run: .\$($exeFile.Name) --help"
        Write-Host "  2. Run: .\$($exeFile.Name) -o logs/app.txt"
        Write-Host "  3. Configure appsettings.json for your logging preferences"
    }
    else {
        $dllFile = Get-ChildItem -Path $OutputPath -Filter "MT5Bridge.Serilog.Demo.dll" -File | Select-Object -First 1
        if ($dllFile) {
            Write-ColorOutput "DLL File: $($dllFile.Name)" "Green"
            Write-Host ""
            Write-ColorOutput "Usage Instructions:" "Yellow"
            Write-Host "  1. Ensure .NET 8.0 Runtime is installed"
            Write-Host "  2. Run: dotnet $($dllFile.Name) --help"
            Write-Host "  3. Run: dotnet $($dllFile.Name) -o logs/app.txt"
            Write-Host "  4. Configure appsettings.json for your logging preferences"
        }
    }
    
    Write-Host ""
    Write-ColorOutput "Important Notes:" "Yellow"
    Write-Host "  - Serilog demonstration application"
    Write-Host "  - Multiple logging sinks supported (Console, File, CloudWatch)"
    Write-Host "  - Configure appsettings.json before first run"

}
catch {
    Write-Host ""
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "Build Failed" "Red"
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "Error: $_" "Red"
    exit 1
}
