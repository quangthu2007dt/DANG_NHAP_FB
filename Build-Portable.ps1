param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SelfContained = $true,
    [string]$CloneDirectory = "D:\DANG NHAP FB CLEAN V2"
)

$ErrorActionPreference = "Stop"

function Invoke-WithRetry {
    param(
        [scriptblock]$Action,
        [int]$MaxRetries = 5,
        [int]$DelayMilliseconds = 1000,
        [string]$Description = "operation"
    )

    for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
        try {
            & $Action
            return
        }
        catch {
            if ($attempt -ge $MaxRetries) {
                throw
            }

            Write-Host "$Description that bai lan $attempt, thu lai..."
            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$appProject = Join-Path $repoRoot "DANG NHAP FACEBOOK\DANG NHAP FACEBOOK.csproj"
$updaterProject = Join-Path $repoRoot "Updater\Updater.csproj"
$versionFile = Join-Path $repoRoot "DANG NHAP FACEBOOK\version.json"
$manifestFile = Join-Path $repoRoot "release\stable\manifest.json"

if (-not (Test-Path $versionFile)) {
    throw "Khong tim thay version.json tai: $versionFile"
}

if (-not (Test-Path $manifestFile)) {
    throw "Khong tim thay release/stable/manifest.json tai: $manifestFile"
}

$versionInfo = Get-Content $versionFile -Raw | ConvertFrom-Json
$manifestInfo = Get-Content $manifestFile -Raw | ConvertFrom-Json

$version = [string]$versionInfo.version
$packageFileName = [string]$manifestInfo.packageFileName

if ([string]::IsNullOrWhiteSpace($version)) {
    throw "version.json chua co version hop le."
}

if ([string]::IsNullOrWhiteSpace($packageFileName)) {
    throw "manifest.json chua co packageFileName hop le."
}

$artifactsRoot = Join-Path $repoRoot "artifacts"
$portableRoot = Join-Path $artifactsRoot "portable"
$portableDirectory = Join-Path $portableRoot $version
$updaterPublishDirectory = Join-Path $artifactsRoot ("updater_publish\" + $version)
$packagesDirectory = Join-Path $artifactsRoot "packages"
$releaseStableDirectory = Join-Path $repoRoot "release\stable"
$releasePackagePath = Join-Path $releaseStableDirectory $packageFileName
$artifactPackagePath = Join-Path $packagesDirectory $packageFileName

New-Item -ItemType Directory -Force -Path $portableRoot | Out-Null
New-Item -ItemType Directory -Force -Path $packagesDirectory | Out-Null
New-Item -ItemType Directory -Force -Path $releaseStableDirectory | Out-Null

if (Test-Path $portableDirectory) {
    Remove-Item -Recurse -Force $portableDirectory
}

if (Test-Path $updaterPublishDirectory) {
    Remove-Item -Recurse -Force $updaterPublishDirectory
}

if (Test-Path $releasePackagePath) {
    Remove-Item -Force $releasePackagePath
}

if (Test-Path $artifactPackagePath) {
    Remove-Item -Force $artifactPackagePath
}

$selfContainedFlag = if ($SelfContained) { "true" } else { "false" }

dotnet publish $appProject -c $Configuration -r $RuntimeIdentifier --self-contained:$selfContainedFlag -p:PublishSingleFile=false -p:SkipBuildAndCopyUpdater=true -o $portableDirectory
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish app that bai."
}

dotnet publish $updaterProject -c $Configuration -r $RuntimeIdentifier --self-contained:$selfContainedFlag -p:PublishSingleFile=false -o $updaterPublishDirectory
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish updater that bai."
}

Copy-Item -Path (Join-Path $updaterPublishDirectory "*") -Destination $portableDirectory -Recurse -Force

foreach ($folderName in @("data", "logs", "temp", "packages")) {
    New-Item -ItemType Directory -Force -Path (Join-Path $portableDirectory $folderName) | Out-Null
}

Invoke-WithRetry -Description "Tao goi zip portable" -Action {
    if (Test-Path $releasePackagePath) {
        Remove-Item -Force $releasePackagePath
    }

    & tar.exe -a -c -f $releasePackagePath -C $portableDirectory .
    if ($LASTEXITCODE -ne 0) {
        throw "tar.exe tao zip that bai, exit code: $LASTEXITCODE"
    }
}

Invoke-WithRetry -Description "Copy package vao artifacts" -Action {
    Copy-Item -Path $releasePackagePath -Destination $artifactPackagePath -Force
}

if (-not [string]::IsNullOrWhiteSpace($CloneDirectory)) {
    if (Test-Path $CloneDirectory) {
        $backupDirectory = "{0}_backup_{1}" -f $CloneDirectory, (Get-Date -Format "yyyyMMdd_HHmmss")
        Move-Item -Path $CloneDirectory -Destination $backupDirectory
        Write-Host "Da doi ten ban cu sang: $backupDirectory"
    }

    Copy-Item -Path $portableDirectory -Destination $CloneDirectory -Recurse
}

Write-Host "Portable build hoan tat."
Write-Host "Version             : $version"
Write-Host "Portable directory  : $portableDirectory"
Write-Host "Release package     : $releasePackagePath"
Write-Host "Artifact package    : $artifactPackagePath"
Write-Host "Clone directory     : $CloneDirectory"
