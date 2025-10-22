@echo off
setlocal enabledelayedexpansion
echo ========================================
echo Starting STMS Backend API
echo ========================================
echo.

REM Navigate to API project dir
cd /d c:\Users\Cooray\OneDrive\Desktop\stms\Backend\STMS.Api

REM Kill any existing STMS.Api bound to port 5000 (best-effort)
for /f "tokens=5" %%p in ('netstat -ano ^| findstr :5000 ^| findstr LISTENING') do (
	echo Found process listening on 5000: PID %%p
	taskkill /F /PID %%p >nul 2>&1
)

echo Starting .NET backend on fixed URL: http://localhost:5000
echo (Using: dotnet run --no-launch-profile --urls http://localhost:5000)
echo.
echo Once started, open: http://localhost:5000/swagger
echo Press Ctrl+C in this window to stop the backend.
echo.

dotnet run --no-launch-profile --urls http://localhost:5000
endlocal
