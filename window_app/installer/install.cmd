@echo off
setlocal

net session >nul 2>&1
if not "%errorlevel%"=="0" (
  powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" -PackageZip "%~dp0VideoWallPlayer-Windows-Portable-x64.zip" -UninstallScript "%~dp0uninstall.ps1"
if errorlevel 1 (
  echo.
  echo VideoWallPlayer kurulumu tamamlanamadi.
  pause
  exit /b 1
)

exit /b 0
