# IncrementVersion.ps1
# This script increments the patch version in both KleiKodeshRibbon.cs and MainWindow.xaml.cs

param(
    [string]$SolutionDir
)

$ribbonFile = Join-Path $SolutionDir "KleiKodesh\Ribbon\KleiKodeshRibbon.cs"
$mainWindowFile = Join-Path $SolutionDir "KleiKodeshInstaller\MainWindow.xaml.cs"

function Increment-Version {
    param([string]$version)
    
    # Extract version number (remove 'v' prefix if present)
    $versionNum = $version -replace '^v', ''
    
    # Split into parts
    $parts = $versionNum.Split('.')
    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patch = [int]$parts[2]
    
    # Increment patch
    $patch++
    
    # Return new version
    return "$major.$minor.$patch"
}

# Read and update KleiKodeshRibbon.cs
$ribbonContent = Get-Content $ribbonFile -Raw -Encoding UTF8
if ($ribbonContent -match 'string version = "v([\d\.]+)"') {
    $currentVersion = $matches[1]
    $newVersion = Increment-Version $currentVersion
    $ribbonContent = $ribbonContent -replace '(string version = )"v[\d\.]+"', "`$1`"v$newVersion`""
    [System.IO.File]::WriteAllText($ribbonFile, $ribbonContent, [System.Text.Encoding]::UTF8)
    Write-Host "Updated KleiKodeshRibbon.cs: v$currentVersion -> v$newVersion"
} else {
    Write-Host "Could not find version in KleiKodeshRibbon.cs"
    exit 1
}

# Read and update MainWindow.xaml.cs
$mainWindowContent = Get-Content $mainWindowFile -Raw -Encoding UTF8
if ($mainWindowContent -match 'const string Version = "([\d\.]+)"') {
    $currentVersionMW = $matches[1]
    $mainWindowContent = $mainWindowContent -replace '(const string Version = )"[\d\.]+"', "`$1`"$newVersion`""
    [System.IO.File]::WriteAllText($mainWindowFile, $mainWindowContent, [System.Text.Encoding]::UTF8)
    Write-Host "Updated MainWindow.xaml.cs: $currentVersionMW -> $newVersion"
} else {
    Write-Host "Could not find version in MainWindow.xaml.cs"
    exit 1
}

Write-Host "Version increment complete: v$newVersion"
