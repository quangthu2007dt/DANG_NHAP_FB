@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "PS_SCRIPT=%SCRIPT_DIR%PHAT_HANH_1_CLICK.ps1"

if not exist "%PS_SCRIPT%" (
  echo Khong tim thay script: %PS_SCRIPT%
  pause
  exit /b 1
)

powershell -NoProfile -ExecutionPolicy Bypass -File "%PS_SCRIPT%"
if errorlevel 1 (
  echo.
  echo PHAT HANH THAT BAI. Xem loi o cua so tren.
  pause
  exit /b 1
)

echo.
echo PHAT HANH HOAN TAT.
pause
exit /b 0
