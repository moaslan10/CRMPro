@echo off
title CRMPro Backend - FINAL FIX
cd /d "%~dp0Backend"

echo.
echo ==========================================
echo       CRMPro Backend - FINAL FIX
echo ==========================================
echo.

for /f "tokens=5" %%a in ('netstat -ano ^| findstr :5002 ^| findstr LISTENING') do (
    echo Stopping old process %%a on port 5002...
    taskkill /PID %%a /F >nul 2>&1
)

if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo Restoring packages...
dotnet restore
if errorlevel 1 (
    echo.
    echo RESTORE FAILED.
    pause
    exit /b 1
)

echo.
echo Starting the FINAL CRMPro API on:
echo http://localhost:5002
echo.
dotnet run --project "%~dp0Backend\CRMPro.Api.csproj" --urls "http://localhost:5002"

pause
