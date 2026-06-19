param(
  [string] $Runtime = "win-x64",
  [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$windowRoot = Join-Path $root "window_app"
$project = Join-Path $windowRoot "native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj"
$publishDir = Join-Path $windowRoot "dist-native"
$packageDir = Join-Path $windowRoot "dist-packages"
$installerDir = Join-Path $windowRoot "dist-installer"
$releaseDir = Join-Path $root "releases"
$portableName = "VideoWallPlayer-Windows-Portable-x64.zip"
$setupName = "VideoWallPlayer-Windows-Setup-x64.exe"
$portablePath = Join-Path $packageDir $portableName
$setupPath = Join-Path $installerDir $setupName
$stagingDir = Join-Path $env:TEMP ("videowallplayer-installer-" + [guid]::NewGuid().ToString("N"))

New-Item -ItemType Directory -Force $publishDir, $packageDir, $installerDir, $releaseDir | Out-Null

Write-Host "==> Publishing $Runtime"
dotnet publish $project -c $Configuration -r $Runtime --self-contained true -o $publishDir
if ($LASTEXITCODE -ne 0) {
  throw "dotnet publish failed with exit code $LASTEXITCODE"
}

Write-Host "==> Building portable package"
if (Test-Path -LiteralPath $portablePath) {
  Remove-Item -LiteralPath $portablePath -Force
}
Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $portablePath -CompressionLevel Optimal

Write-Host "==> Preparing setup package"
if (Test-Path -LiteralPath $stagingDir) {
  Remove-Item -LiteralPath $stagingDir -Recurse -Force
}
New-Item -ItemType Directory -Force $stagingDir | Out-Null
Copy-Item -LiteralPath $portablePath -Destination (Join-Path $stagingDir $portableName) -Force
Copy-Item -LiteralPath (Join-Path $windowRoot "installer\install.cmd") -Destination (Join-Path $stagingDir "install.cmd") -Force
Copy-Item -LiteralPath (Join-Path $windowRoot "installer\install.ps1") -Destination (Join-Path $stagingDir "install.ps1") -Force
Copy-Item -LiteralPath (Join-Path $windowRoot "installer\uninstall.ps1") -Destination (Join-Path $stagingDir "uninstall.ps1") -Force

$sedPath = Join-Path $stagingDir "VideoWallPlayerSetup.sed"
$sed = @"
[Version]
Class=IEXPRESS
SEDVersion=3
[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=1
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=$setupPath
FriendlyName=VideoWallPlayer Setup
AppLaunched=%FILE0%
PostInstallCmd=<None>
AdminQuietInstCmd=%FILE0%
UserQuietInstCmd=%FILE0%
SourceFiles=SourceFiles
[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=VideoWallPlayer installation is complete.
FILE0=install.cmd
FILE1=install.ps1
FILE2=uninstall.ps1
FILE3=$portableName
[SourceFiles]
SourceFiles0=$stagingDir\
[SourceFiles0]
%FILE0%=
%FILE1%=
%FILE2%=
%FILE3%=
"@
Set-Content -Path $sedPath -Value $sed -Encoding ASCII

Write-Host "==> Building setup executable"
if (Test-Path -LiteralPath $setupPath) {
  Remove-Item -LiteralPath $setupPath -Force
}
$iexpressStart = Get-Date
iexpress.exe /N /Q $sedPath

$deadline = (Get-Date).AddMinutes(10)
do {
  Start-Sleep -Seconds 5
  $setupExists = Test-Path -LiteralPath $setupPath
  $activeMakeCab = Get-Process makecab -ErrorAction SilentlyContinue |
    Where-Object { $_.StartTime -ge $iexpressStart.AddSeconds(-2) }
} while ((-not $setupExists -or $activeMakeCab) -and (Get-Date) -lt $deadline)

if (-not (Test-Path -LiteralPath $setupPath)) {
  throw "Setup package was not created: $setupPath"
}
if ($activeMakeCab) {
  throw "Setup package build did not finish before timeout: $setupPath"
}

Copy-Item -LiteralPath $portablePath -Destination (Join-Path $releaseDir $portableName) -Force
Copy-Item -LiteralPath $setupPath -Destination (Join-Path $releaseDir $setupName) -Force

Remove-Item -LiteralPath $stagingDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "==> Done"
Get-Item $portablePath, $setupPath, (Join-Path $releaseDir $portableName), (Join-Path $releaseDir $setupName) |
  Select-Object FullName, Length
