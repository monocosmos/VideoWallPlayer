param(
  [string] $Runtime = "win-x64",
  [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$windowRoot = Join-Path $root "window_app"
$project = Join-Path $windowRoot "native\VideoWallPlayer.Native\VideoWallPlayer.Native.csproj"
$setupProject = Join-Path $windowRoot "installer\VideoWallPlayer.Setup\VideoWallPlayer.Setup.csproj"
$publishDir = Join-Path $windowRoot "dist-native"
$packageDir = Join-Path $windowRoot "dist-packages"
$installerDir = Join-Path $windowRoot "dist-installer"
$setupPublishDir = Join-Path $installerDir "publish"
$setupPayloadDir = Join-Path $windowRoot "installer\VideoWallPlayer.Setup\Payload"
$setupStaticPayloadPath = Join-Path $setupPayloadDir "Payload.zip"
$releaseDir = Join-Path $root "releases"
$portableName = "VideoWallPlayer-Windows-Portable-x64.zip"
$setupName = "VideoWallPlayer-Windows-Setup-x64.exe"
$portablePath = Join-Path $packageDir $portableName
$setupPath = Join-Path $installerDir $setupName

New-Item -ItemType Directory -Force $publishDir, $packageDir, $installerDir, $setupPublishDir, $setupPayloadDir, $releaseDir | Out-Null

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

Write-Host "==> Building setup executable"
if (Test-Path -LiteralPath $setupPath) {
  Remove-Item -LiteralPath $setupPath -Force
}

if (Test-Path -LiteralPath $setupPublishDir) {
  Remove-Item -LiteralPath $setupPublishDir -Recurse -Force
}
New-Item -ItemType Directory -Force $setupPublishDir | Out-Null

Copy-Item -LiteralPath $portablePath -Destination (Join-Path $setupPayloadDir $portableName) -Force
Copy-Item -LiteralPath $portablePath -Destination $setupStaticPayloadPath -Force

dotnet publish $setupProject `
  -c $Configuration `
  -r $Runtime `
  --self-contained true `
  -o $setupPublishDir `
  -p:InstallerPayloadZipPath="$portablePath" `
  -p:InstallerRuntimeIdentifier=$Runtime `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:DebugType=none `
  -p:DebugSymbols=false
if ($LASTEXITCODE -ne 0) {
  throw "setup publish failed with exit code $LASTEXITCODE"
}

$builtSetupPath = Join-Path $setupPublishDir "VideoWallPlayerSetup.exe"
if (-not (Test-Path -LiteralPath $builtSetupPath)) {
  throw "Setup package was not created: $builtSetupPath"
}

Copy-Item -LiteralPath $builtSetupPath -Destination $setupPath -Force
Copy-Item -LiteralPath $portablePath -Destination (Join-Path $releaseDir $portableName) -Force
Copy-Item -LiteralPath $setupPath -Destination (Join-Path $releaseDir $setupName) -Force

Write-Host "==> Done"
Get-Item $portablePath, $setupPath, (Join-Path $releaseDir $portableName), (Join-Path $releaseDir $setupName) |
  Select-Object FullName, Length
