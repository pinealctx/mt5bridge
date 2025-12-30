<#
.SYNOPSIS
    MT5Bridge.Manager.Demo Release Script for Windows x64

.DESCRIPTION
    Parametric packaging script for Windows 64-bit platform, supports Debug/Release and self-contained options

.PARAMETER Configuration
    Build configuration (Debug or Release), default is Release

.PARAMETER SelfContained
    Whether to publish as self-contained, default is false (framework-dependent)

.PARAMETER OutputPath
    Output path, default is .\publish

.PARAMETER Clean
    Clean output directory before publishing

.PARAMETER CopyMT5Libs
    Whether to copy MT5 SDK library files, default is true

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
    
    [Parameter(HelpMessage = "Copy MT5 SDK library files")]
    [switch]$CopyMT5Libs = $true,
    
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
    Write-ColorOutput "  MT5Bridge.Manager.Demo Build Script" "Cyan"
    Write-ColorOutput "  Target Platform: Windows x64" "Cyan"
    Write-ColorOutput "========================================" "Cyan"
    Write-Host ""
    Write-ColorOutput "Build Parameters:" "Yellow"
    Write-Host "  - Configuration: $Configuration"
    Write-Host "  - Self-Contained: $(if ($SelfContained) { 'Yes' } else { 'No (Framework-Dependent)' })"
    Write-Host "  - Single File: $(if ($SingleFile) { 'Yes' } else { 'No' })"
    Write-Host "  - Output Path: $OutputPath"
    Write-Host "  - Clean Mode: $(if ($Clean) { 'Yes' } else { 'No' })"
    Write-Host "  - Copy MT5 Libs: $(if ($CopyMT5Libs) { 'Yes' } else { 'No' })"
    Write-Host "  - Include Symbols: $(if ($IncludeSymbols) { 'Yes (PDB files)' } else { 'No' })"
    Write-Host ""

    $projectFile = Join-Path $PSScriptRoot "MT5Bridge.Manager.Demo.csproj"
    
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
        "-r", "win-x64",
        "-p:Platform=x64",
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

    if ($CopyMT5Libs) {
        Write-Host ""
        Write-ColorOutput "Copying MT5 SDK library files..." "Yellow"
        
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
                Write-Host "  Copied: $lib" -ForegroundColor Green
                $copiedCount++
            }
            else {
                Write-Host "  Not found: $lib" -ForegroundColor Yellow
            }
        }
        
        if ($copiedCount -gt 0) {
            Write-ColorOutput "Copied $copiedCount MT5 SDK library files" "Green"
        }
    }

    Write-Host ""
    Write-ColorOutput "Copying configuration files..." "Yellow"
    
    # Check if actual config files exist
    $hasActualConfig = (Test-Path (Join-Path $PSScriptRoot "appsettings.json")) -or 
    (Test-Path (Join-Path $PSScriptRoot "nlog.json"))
    
    if ($hasActualConfig) {
        # If actual config exists, copy them (not the examples)
        $configFiles = @("appsettings.json", "nlog.json")
        $copiedConfigs = 0
        foreach ($file in $configFiles) {
            $sourcePath = Join-Path $PSScriptRoot $file
            $destPath = Join-Path $OutputPath $file
            
            if (Test-Path $sourcePath) {
                Copy-Item -Path $sourcePath -Destination $destPath -Force
                Write-Host "  Copied: $file" -ForegroundColor Green
                $copiedConfigs++
            }
        }
        
        if ($copiedConfigs -gt 0) {
            Write-Host ""
            Write-ColorOutput "Using actual config files from project directory" "Cyan"
        }
    }
    else {
        # If no actual config, copy example files
        $exampleFiles = @("appsettings.example.json", "nlog.example.json")
        foreach ($file in $exampleFiles) {
            $sourcePath = Join-Path $PSScriptRoot $file
            $destPath = Join-Path $OutputPath $file
            
            if (Test-Path $sourcePath) {
                Copy-Item -Path $sourcePath -Destination $destPath -Force
                Write-Host "  Copied: $file" -ForegroundColor Green
            }
        }
        
        Write-Host ""
        Write-ColorOutput "Note: Using example config files" "Yellow"
        Write-Host "  You need to copy and configure them:"
        Write-Host "    1. Copy appsettings.example.json -> appsettings.json"
        Write-Host "    2. Edit appsettings.json with your MT5 connection info"
        Write-Host "    3. (Optional) Copy nlog.example.json -> nlog.json"
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
        Write-Host "  1. Copy appsettings.example.json to appsettings.json"
        Write-Host "  2. Edit appsettings.json with your MT5 connection info"
        Write-Host "  3. (Optional) Modify nlog.example.json and rename to nlog.json"
        Write-Host "  4. Run: .\$($exeFile.Name) --help"
    }
    else {
        $dllFile = Get-ChildItem -Path $OutputPath -Filter "MT5Bridge.Manager.Demo.dll" -File | Select-Object -First 1
        if ($dllFile) {
            Write-ColorOutput "DLL File: $($dllFile.Name)" "Green"
            Write-Host ""
            Write-ColorOutput "Usage Instructions:" "Yellow"
            Write-Host "  1. Ensure .NET 8.0 Runtime is installed"
            Write-Host "  2. Copy appsettings.example.json to appsettings.json"
            Write-Host "  3. Edit appsettings.json with your MT5 connection info"
            Write-Host "  4. (Optional) Modify nlog.example.json and rename to nlog.json"
            Write-Host "  5. Run: dotnet $($dllFile.Name) --help"
        }
    }
    
    Write-Host ""
    Write-ColorOutput "Important Notes:" "Yellow"
    Write-Host "  - MT5 Manager API runtime environment required on target machine"
    Write-Host "  - MT5 SDK library files copied to output directory"
    Write-Host "  - Please configure appsettings.json before first run"

}
catch {
    Write-Host ""
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "Build Failed" "Red"
    Write-ColorOutput "========================================" "Red"
    Write-ColorOutput "Error: $_" "Red"
    exit 1
}