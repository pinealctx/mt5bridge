# This script generates C# classes from .proto files using protoc.
# You need to have protoc installed and in your PATH.
# Alternatively, you can use the Grpc.Tools NuGet package in your project.

$ProtoFile = "mt5_models.proto"
$OutDir = "../Models/Proto"

if (-not (Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir -Force
}

Write-Host "Generating C# classes from $ProtoFile..."

# Try to find protoc
$Protoc = Get-Command protoc -ErrorAction SilentlyContinue

if ($null -eq $Protoc) {
    Write-Error "protoc not found in PATH. Please install Protocol Buffers compiler."
    Write-Host "You can download it from: https://github.com/protocolbuffers/protobuf/releases"
    Write-Host "Or install via chocolatey: choco install protoc"
    exit 1
}

& $Protoc.Source --proto_path=. --csharp_out=$OutDir $ProtoFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "Successfully generated C# classes in $OutDir"
}
else {
    Write-Error "Failed to generate C# classes."
}
