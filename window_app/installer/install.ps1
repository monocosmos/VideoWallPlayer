param(
  [Parameter(Mandatory = $true)]
  [string] $PackageZip,

  [Parameter(Mandatory = $true)]
  [string] $UninstallScript
)

$ErrorActionPreference = "Stop"

$appName = "VideoWallPlayer"
$publisher = "Nodera Software"
$version = "1.0.0"
$installDir = Join-Path $env:ProgramFiles $appName
$exePath = Join-Path $installDir "VideoWallPlayer.exe"
$uninstallTarget = Join-Path $installDir "uninstall.ps1"

if (-not (Test-Path -LiteralPath $PackageZip)) {
  throw "Portable package not found: $PackageZip"
}

if (Test-Path -LiteralPath $installDir) {
  Remove-Item -LiteralPath $installDir -Recurse -Force
}

New-Item -ItemType Directory -Path $installDir -Force | Out-Null
Expand-Archive -LiteralPath $PackageZip -DestinationPath $installDir -Force
Copy-Item -LiteralPath $UninstallScript -Destination $uninstallTarget -Force

if (-not (Test-Path -LiteralPath $exePath)) {
  throw "Installed executable not found: $exePath"
}

$shell = New-Object -ComObject WScript.Shell
$startMenuDir = Join-Path $env:ProgramData "Microsoft\Windows\Start Menu\Programs\$appName"
New-Item -ItemType Directory -Path $startMenuDir -Force | Out-Null

$startShortcut = $shell.CreateShortcut((Join-Path $startMenuDir "$appName.lnk"))
$startShortcut.TargetPath = $exePath
$startShortcut.WorkingDirectory = $installDir
$startShortcut.IconLocation = $exePath
$startShortcut.Save()

$desktopDir = [Environment]::GetFolderPath("CommonDesktopDirectory")
if ($desktopDir) {
  $desktopShortcut = $shell.CreateShortcut((Join-Path $desktopDir "$appName.lnk"))
  $desktopShortcut.TargetPath = $exePath
  $desktopShortcut.WorkingDirectory = $installDir
  $desktopShortcut.IconLocation = $exePath
  $desktopShortcut.Save()
}

$uninstallKey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\$appName"
New-Item -Path $uninstallKey -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "DisplayName" -Value $appName -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "DisplayVersion" -Value $version -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "Publisher" -Value $publisher -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "InstallLocation" -Value $installDir -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "DisplayIcon" -Value $exePath -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "UninstallString" -Value "`"$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe`" -NoProfile -ExecutionPolicy Bypass -File `"$uninstallTarget`"" -PropertyType String -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "NoModify" -Value 1 -PropertyType DWord -Force | Out-Null
New-ItemProperty -Path $uninstallKey -Name "NoRepair" -Value 1 -PropertyType DWord -Force | Out-Null

$estimatedSizeKb = [int]((Get-ChildItem -LiteralPath $installDir -Recurse -File | Measure-Object Length -Sum).Sum / 1KB)
New-ItemProperty -Path $uninstallKey -Name "EstimatedSize" -Value $estimatedSizeKb -PropertyType DWord -Force | Out-Null

Write-Host "$appName installed to $installDir"
