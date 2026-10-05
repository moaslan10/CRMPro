@echo off
title CRMPro Frontend
cd /d "%~dp0Frontend"
echo.
echo ===== CRMPro Frontend =====
echo.
if not exist node_modules (
  echo Installing npm packages...
  npm install
  if errorlevel 1 (
    echo.
    echo npm install failed.
    pause
    exit /b 1
  )
)
npm run dev
pause
