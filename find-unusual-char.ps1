#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Find emoji and Unicode special characters in C# source code files

.DESCRIPTION
    Recursively searches all *.cs files for emoji characters and other special Unicode symbols
    and outputs results in a format that can be clicked in VSCode terminal to jump to the location.

.PARAMETER Mode
    Detection mode:
    - 'emoji' (default): Common emoji only (✅, ❌, 🚀, etc.)
    - 'unicode': All non-ASCII characters including ✓, ►, —, etc.
    - 'visible': Only visible non-ASCII characters (excludes control/format chars)

.EXAMPLE
    .\find-emoji.ps1
    Find common emoji in C# files

.EXAMPLE
    .\find-emoji.ps1 -Mode unicode
    Find all special Unicode characters

.EXAMPLE
    .\find-emoji.ps1 -Mode visible
    Find only visible non-ASCII characters

.EXAMPLE
    .\find-emoji.ps1 -Verbose
    Find emoji with verbose output
#>

param(
    [ValidateSet('emoji', 'unicode', 'visible')]
    [string]$Mode = 'emoji',
    
    [switch]$Verbose
)

# Common emoji that might appear in code
$emojiList = @(
    '✅', '❌', '⭐', '🚀', '📝', '🛠', '📊', '🏗', '🔌', '🔗', '⚡', '📖', '✨', '🎯', '📋', '⚠', '💡', '🔍',
    '💻', '🔧', '📁', '📂', '🎨', '✏️', '🖊️', '📌', '📍', '🔔', '🔕', '✉️', '📧', '💬', '🗨️', '📞', '☎️',
    '⏰', '⏱️', '⏲️', '🕐', '🕑', '⌚', '⌛', '⏳', '📅', '📆', '🗓️', '📇', '📈', '📉', '📊', '💰', '💳'
)

# Special characters (arrows, checkmarks, dashes, etc.)
$specialCharList = @(
    '✓', '✗', '✔', '✘', '►', '◄', '▼', '▲', '→', '←', '↑', '↓',
    '—', '–', '•', '°', '™', '©', '®', '…', '§', '¶', '†', '‡'
)

Write-Host "Scanning for special characters in C# files..." -ForegroundColor Yellow
Write-Host "Mode: $Mode" -ForegroundColor Cyan
Write-Host ""

$foundCount = 0
$fileCount = 0
$foundItems = @()

# Get all .cs files
$csFiles = Get-ChildItem -Path . -Recurse -Include "*.cs" -File | Where-Object {
    # Exclude bin, obj, and .git folders
    $_.FullName -notmatch '\\bin\\' -and 
    $_.FullName -notmatch '\\obj\\' -and 
    $_.FullName -notmatch '\\.git\\' -and
    $_.FullName -notmatch '\\\.'
}

foreach ($file in $csFiles) {
    $fileCount++
    
    try {
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
        $lines = $content -split "`n"
        
        $lineNumber = 0
        foreach ($line in $lines) {
            $lineNumber++
            
            $hasMatch = $false
            $foundChars = @()
            
            if ($Mode -eq 'emoji') {
                # Check if line contains any emoji
                foreach ($emoji in $emojiList) {
                    if ($line.Contains($emoji)) {
                        $hasMatch = $true
                        $foundChars += $emoji
                    }
                }
            }
            elseif ($Mode -eq 'unicode') {
                # Check for all non-ASCII characters
                foreach ($char in $line.ToCharArray()) {
                    $charCode = [int][char]$char
                    
                    # If character is outside ASCII range (0-127)
                    if ($charCode -gt 127) {
                        $hasMatch = $true
                        $foundChars += $char
                    }
                }
                
                # Remove duplicates
                $foundChars = $foundChars | Select-Object -Unique
            }
            elseif ($Mode -eq 'visible') {
                # Check for visible non-ASCII characters only
                # Exclude control characters and formatting characters
                foreach ($char in $line.ToCharArray()) {
                    $charCode = [int][char]$char
                    
                    # If character is outside ASCII range (0-127)
                    if ($charCode -gt 127) {
                        try {
                            $charType = [System.Globalization.CharUnicodeInfo]::GetUnicodeCategory($char)
                            
                            # Include only printable characters
                            # Exclude: Control, Separator, Format chars
                            if ($charType -ne 'Control' -and $charType -ne 'Separator' -and $charType -ne 'Format') {
                                $hasMatch = $true
                                $foundChars += $char
                            }
                        }
                        catch {
                            # If we can't determine category, include it
                            $hasMatch = $true
                            $foundChars += $char
                        }
                    }
                }
                
                # Remove duplicates
                $foundChars = $foundChars | Select-Object -Unique
            }
            
            if ($hasMatch) {
                $relativePath = Resolve-Path -Path $file.FullName -Relative
                
                # Output in VSCode clickable format: file.cs:line:col
                $charStr = if ($foundChars.Count -gt 0) { " [$($foundChars -join ' ')]" } else { "" }
                Write-Host "$relativePath`:$lineNumber`:0" -ForegroundColor Cyan -NoNewline
                Write-Host "$charStr " -ForegroundColor Yellow -NoNewline
                Write-Host "$($line.Trim())" -ForegroundColor White
                
                $foundCount++
                
                $foundItems += [PSCustomObject]@{
                    File       = $relativePath
                    Line       = $lineNumber
                    Characters = $foundChars -join ' '
                }
            }
        }
    }
    catch {
        if ($Verbose) {
            Write-Warning "Failed to process $($file.FullName): $($_.Exception.Message)"
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Scan Complete" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Files scanned: $fileCount" -ForegroundColor Cyan
Write-Host "Special chars found: $foundCount" -ForegroundColor $(if ($foundCount -gt 0) { 'Yellow' } else { 'Green' })
Write-Host ""

if ($foundCount -gt 0) {
    Write-Host "TIP: You can click on the file paths above in VSCode terminal to jump to those lines" -ForegroundColor Green
    Write-Host ""
    
    if ($Verbose) {
        Write-Host "Summary:" -ForegroundColor Yellow
        $foundItems | Format-Table -AutoSize
    }
}
