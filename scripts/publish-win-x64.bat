@echo off
setlocal
cd /d "%~dp0.."
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish-win-x64.ps1"
if errorlevel 1 (
  echo.
  echo Publish islemi basarisiz oldu.
  pause
  exit /b 1
)
echo.
echo Publish tamamlandi.
pause
