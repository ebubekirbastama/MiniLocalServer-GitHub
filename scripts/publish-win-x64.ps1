param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "src\MiniLocalServer\MiniLocalServer.csproj"
$output = Join-Path $repoRoot "artifacts\win-x64"
$zip = Join-Path $repoRoot "artifacts\MiniLocalServer-win-x64.zip"

Write-Host "MiniLocalServer Windows x64 publish basliyor..." -ForegroundColor Cyan
Write-Host "Project: $project"
Write-Host "Output : $output"

if (Test-Path $output) {
    Remove-Item $output -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $output | Out-Null

dotnet publish $project `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    -o $output

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish basarisiz oldu."
}

if (Test-Path $zip) {
    Remove-Item $zip -Force
}
Compress-Archive -Path (Join-Path $output "*") -DestinationPath $zip -Force

Write-Host "" 
Write-Host "Tamamlandi." -ForegroundColor Green
Write-Host "EXE: $(Join-Path $output 'MiniLocalServer.exe')"
Write-Host "ZIP: $zip"
