# Requires -Version 5.1

<#
.SYNOPSIS
    Recursively removes UTF-8 BOM (Byte Order Mark) from text files.

.DESCRIPTION
    Traverses the current directory and subdirectories to find files with a UTF-8 BOM.
    Rewrites them as UTF-8 without BOM.
    Excludes directories starting with a dot (e.g., .vs, .vscode, .git) and common build folders (bin, obj).
#>

$utf8NoBom = New-Object System.Text.UTF8Encoding $false

Write-Host "Scanning for files with UTF-8 BOM..." -ForegroundColor Yellow

# Get all files recursively
$files = Get-ChildItem -Path . -Recurse -File | Where-Object {
    # Exclude directories starting with a dot (e.g. .vs, .vscode, .git, .doc)
    # Also exclude bin and obj folders which are common in .NET projects
    $_.FullName -notmatch '\\\.' -and 
    $_.FullName -notmatch '\\bin\\' -and 
    $_.FullName -notmatch '\\obj\\'
}

$count = 0
foreach ($file in $files) {
    try {
        # Read the first 3 bytes to check for UTF-8 BOM (0xEF, 0xBB, 0xBF)
        $stream = [System.IO.File]::OpenRead($file.FullName)
        $bytes = New-Object byte[] 3
        $read = $stream.Read($bytes, 0, 3)
        $stream.Close()

        if ($read -eq 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            Write-Host "Removing BOM: $($file.FullName)" -ForegroundColor Cyan
            
            # Read content and write back without BOM
            $content = [System.IO.File]::ReadAllText($file.FullName)
            [System.IO.File]::WriteAllText($file.FullName, $content, $utf8NoBom)
            
            $count++
        }
    }
    catch {
        Write-Warning "Failed to process $($file.FullName): $($_.Exception.Message)"
    }
}

Write-Host "`nDone. Removed BOM from $count files." -ForegroundColor Green
