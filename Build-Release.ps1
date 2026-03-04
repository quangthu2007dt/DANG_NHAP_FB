param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$appProject = Join-Path $repoRoot "DANG NHAP FACEBOOK\DANG NHAP FACEBOOK.csproj"
$versionFile = Join-Path $repoRoot "DANG NHAP FACEBOOK\version.json"
$manifestFile = Join-Path $repoRoot "DANG NHAP FACEBOOK\manifest.json"
$artifactsRoot = Join-Path $repoRoot "artifacts"
$releaseRoot = Join-Path $artifactsRoot "release"
$packageRoot = Join-Path $artifactsRoot "packages"
$cloneDirectory = "D:\DANG NHAP FB"

if (-not (Test-Path $versionFile)) {
    throw "Không tìm thấy version.json tại: $versionFile"
}

if (-not (Test-Path $manifestFile)) {
    throw "Không tìm thấy manifest.json tại: $manifestFile"
}

$versionInfo = Get-Content $versionFile -Raw | ConvertFrom-Json
$manifestInfo = Get-Content $manifestFile -Raw | ConvertFrom-Json

$version = [string]$versionInfo.version
$packageFileName = [string]$manifestInfo.packageFileName

if ([string]::IsNullOrWhiteSpace($version)) {
    throw "version.json chưa có version hợp lệ."
}

if ([string]::IsNullOrWhiteSpace($packageFileName)) {
    throw "manifest.json chưa có packageFileName hợp lệ."
}

$releaseDirectory = Join-Path $releaseRoot $version
$packagePath = Join-Path $packageRoot $packageFileName

New-Item -ItemType Directory -Force -Path $releaseRoot | Out-Null
New-Item -ItemType Directory -Force -Path $packageRoot | Out-Null

if (Test-Path $releaseDirectory) {
    Remove-Item -Recurse -Force $releaseDirectory
}

if (Test-Path $packagePath) {
    Remove-Item -Force $packagePath
}

if (Test-Path $cloneDirectory) {
    Remove-Item -Recurse -Force $cloneDirectory
}

dotnet build $appProject -c $Configuration -o $releaseDirectory

Compress-Archive -Path (Join-Path $releaseDirectory '*') -DestinationPath $packagePath -Force
Copy-Item -Path $releaseDirectory -Destination $cloneDirectory -Recurse

Write-Host "Build hoàn tất."
Write-Host "Thư mục release : $releaseDirectory"
Write-Host "Gói update zip  : $packagePath"
Write-Host "Thư mục clone   : $cloneDirectory"
