param(
    [string]$Configuration = "Release",
    [string]$RuntimeIdentifier = "win-x64",
    [switch]$SelfContained = $true,
    [string]$CloneDirectory = "E:\PHAT_HANH_FB_V2_MAU",
    [string]$Notes = "",
    [string]$CommitMessage = "",
    [string]$RemoteName = "origin",
    [switch]$SkipGitPublish,
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

function Get-NextVersion {
    param([string]$CurrentVersion)

    if ([string]::IsNullOrWhiteSpace($CurrentVersion)) {
        throw "Version hien tai rong."
    }

    $trimmed = $CurrentVersion.Trim()
    $prefix = if ($trimmed.StartsWith("V", [System.StringComparison]::OrdinalIgnoreCase)) { "V" } else { "" }
    $numbers = [System.Text.RegularExpressions.Regex]::Matches($trimmed, "\d+") | ForEach-Object { $_.Value }

    if ($numbers.Count -eq 0) {
        throw "Khong parse duoc version: $CurrentVersion"
    }

    if ($numbers.Count -eq 1) {
        $next = ([int]$numbers[0]) + 1
        return "$prefix$next"
    }

    $lastIndex = $numbers.Count - 1
    $lastRaw = $numbers[$lastIndex]
    $nextLast = ([int]$lastRaw) + 1
    $padWidth = [Math]::Max($lastRaw.Length, 2)
    $numbers[$lastIndex] = $nextLast.ToString().PadLeft($padWidth, "0")
    return "$prefix$($numbers -join '.')"
}

function New-PackageFileNameFromVersion {
    param([string]$Version)

    $clean = $Version.Trim() -replace "[^0-9A-Za-z\.]", ""
    if ($clean -notmatch "^[Vv]") {
        $clean = "V$clean"
    }

    $token = $clean -replace "\.", "_"
    return "DANG_NHAP_FACEBOOK_$token.zip"
}

function Write-JsonUtf8NoBom {
    param(
        [string]$Path,
        [object]$Object
    )

    $json = $Object | ConvertTo-Json -Depth 20
    [System.IO.File]::WriteAllText($Path, $json + [Environment]::NewLine, [System.Text.UTF8Encoding]::new($false))
}

function Invoke-ExternalCommand {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$WorkingDirectory,
        [switch]$IgnoreExitCode
    )

    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    if (-not $IgnoreExitCode -and $exitCode -ne 0) {
        throw "Lenh that bai ($exitCode): $FilePath $($Arguments -join ' ')"
    }

    return $exitCode
}

function Get-ExternalOutput {
    param(
        [string]$FilePath,
        [string[]]$Arguments,
        [string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        $output = & $FilePath @Arguments 2>&1
        $exitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }

    if ($exitCode -ne 0) {
        $details = ($output | Out-String).Trim()
        throw "Lenh that bai ($exitCode): $FilePath $($Arguments -join ' ')`n$details"
    }

    return ($output | Out-String).Trim()
}

function Publish-GitRelease {
    param(
        [string]$RepoRoot,
        [string]$BranchName,
        [string]$RemoteName,
        [string]$CommitMessage
    )

    Write-Host ""
    Write-Host "Git branch      : $BranchName"
    Write-Host "Git remote      : $RemoteName"
    Write-Host "Commit message  : $CommitMessage"
    Write-Host "Dang commit va push GitHub..."

    Invoke-ExternalCommand -FilePath "git" -Arguments @("-C", $RepoRoot, "add", "-A") -WorkingDirectory $RepoRoot
    $diffExitCode = Invoke-ExternalCommand -FilePath "git" -Arguments @("-C", $RepoRoot, "diff", "--cached", "--quiet", "--exit-code") -WorkingDirectory $RepoRoot -IgnoreExitCode

    if ($diffExitCode -eq 0) {
        Write-Host "Khong co thay doi de commit."
        return
    }

    if ($diffExitCode -ne 1) {
        throw "Khong kiem tra duoc thay doi staged trong Git."
    }

    Invoke-ExternalCommand -FilePath "git" -Arguments @("-C", $RepoRoot, "commit", "-m", $CommitMessage) -WorkingDirectory $RepoRoot

    try {
        Invoke-ExternalCommand -FilePath "git" -Arguments @("-C", $RepoRoot, "push", $RemoteName, $BranchName) -WorkingDirectory $RepoRoot
    }
    catch {
        throw "Git push that bai. Ban phat hanh da duoc commit local, nhung chua dua len GitHub.`n$($_.Exception.Message)"
    }
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$versionFile = Join-Path $repoRoot "DANG NHAP FACEBOOK\version.json"
$appManifestFile = Join-Path $repoRoot "DANG NHAP FACEBOOK\manifest.json"
$stableManifestFile = Join-Path $repoRoot "release\stable\manifest.json"
$buildScript = Join-Path $repoRoot "Build-Portable.ps1"

foreach ($requiredPath in @($versionFile, $appManifestFile, $stableManifestFile, $buildScript)) {
    if (-not (Test-Path $requiredPath)) {
        throw "Khong tim thay file bat buoc: $requiredPath"
    }
}

$versionInfo = Get-Content $versionFile -Raw | ConvertFrom-Json
$stableManifest = Get-Content $stableManifestFile -Raw | ConvertFrom-Json

$currentVersion = [string]$versionInfo.version
$nextVersion = Get-NextVersion -CurrentVersion $currentVersion
$releaseDate = Get-Date -Format "yyyy-MM-dd"
$packageFileName = New-PackageFileNameFromVersion -Version $nextVersion

$defaultPackageBaseUrl = "https://raw.githubusercontent.com/quangthu2007dt/DANG_NHAP_FB/v2/session-runtime/release/stable/"
$currentPackageUrl = [string]$stableManifest.packageUrl
if ($currentPackageUrl -match "^(.*/)[^/]+$") {
    $packageBaseUrl = $Matches[1]
} else {
    $packageBaseUrl = $defaultPackageBaseUrl
}
$packageUrl = "$packageBaseUrl$packageFileName"

$notesValue = if ([string]::IsNullOrWhiteSpace($Notes)) {
    "Phat hanh $nextVersion tu script 1-click."
} else {
    $Notes.Trim()
}

$gitBranch = ""
$gitRemoteUrl = ""
$commitMessageValue = if ([string]::IsNullOrWhiteSpace($CommitMessage)) {
    "release: $nextVersion via 1-click"
} else {
    $CommitMessage.Trim()
}

if (-not $SkipGitPublish) {
    $gitBranch = Get-ExternalOutput -FilePath "git" -Arguments @("-C", $repoRoot, "branch", "--show-current") -WorkingDirectory $repoRoot
    if ([string]::IsNullOrWhiteSpace($gitBranch)) {
        throw "Khong xac dinh duoc branch Git hien tai."
    }

    $gitRemoteUrl = Get-ExternalOutput -FilePath "git" -Arguments @("-C", $repoRoot, "remote", "get-url", $RemoteName) -WorkingDirectory $repoRoot
}

$newVersionInfo = [ordered]@{
    appName = [string]$versionInfo.appName
    version = $nextVersion
    releaseDate = $releaseDate
    channel = [string]$versionInfo.channel
}

$newManifest = [ordered]@{
    appName = [string]$stableManifest.appName
    channel = [string]$stableManifest.channel
    latestVersion = $nextVersion
    releaseDate = $releaseDate
    packageFileName = $packageFileName
    packageUrl = $packageUrl
    notes = $notesValue
}

Write-Host "Current version : $currentVersion"
Write-Host "Next version    : $nextVersion"
Write-Host "Release date    : $releaseDate"
Write-Host "Package file    : $packageFileName"
Write-Host "Sample output   : $CloneDirectory"
if (-not $SkipGitPublish) {
    Write-Host "Git branch      : $gitBranch"
    Write-Host "Git remote      : $RemoteName"
    Write-Host "Git remote url  : $gitRemoteUrl"
    Write-Host "Commit message  : $commitMessageValue"
} else {
    Write-Host "Git publish     : bo qua theo tuy chon -SkipGitPublish"
}

if ($DryRun) {
    Write-Host "[DRY-RUN] Khong ghi file, khong build."
    return
}

$rawVersionBackup = Get-Content $versionFile -Raw
$rawAppManifestBackup = Get-Content $appManifestFile -Raw
$rawStableManifestBackup = Get-Content $stableManifestFile -Raw

try {
    Write-JsonUtf8NoBom -Path $versionFile -Object $newVersionInfo
    Write-JsonUtf8NoBom -Path $appManifestFile -Object $newManifest
    Write-JsonUtf8NoBom -Path $stableManifestFile -Object $newManifest

    & $buildScript -Configuration $Configuration -RuntimeIdentifier $RuntimeIdentifier -SelfContained:$SelfContained -CloneDirectory $CloneDirectory
}
catch {
    [System.IO.File]::WriteAllText($versionFile, $rawVersionBackup, [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::WriteAllText($appManifestFile, $rawAppManifestBackup, [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::WriteAllText($stableManifestFile, $rawStableManifestBackup, [System.Text.UTF8Encoding]::new($false))
    throw
}

if (-not $SkipGitPublish) {
    Publish-GitRelease -RepoRoot $repoRoot -BranchName $gitBranch -RemoteName $RemoteName -CommitMessage $commitMessageValue
}

Write-Host ""
Write-Host "PHAT HANH THANH CONG"
Write-Host "Version moi      : $nextVersion"
Write-Host "Thu muc mau sach : $CloneDirectory"
if (-not $SkipGitPublish) {
    Write-Host "Da day len Git   : $RemoteName/$gitBranch"
}
