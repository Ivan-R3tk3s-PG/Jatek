@echo off
setlocal
cd /d "%~dp0"
where dotnet >nul 2>nul
if errorlevel 1 (
  echo.
  echo .NET SDK was not found. Install the .NET 8 SDK:
  echo https://dotnet.microsoft.com/download/dotnet/8.0
  echo Then run this launcher again.
  pause
  exit /b 1
)
echo.
echo ===== NEON BREACH / EXTRACTION PROTOCOL =====
echo.
dotnet run --project NEON_BREACH.csproj -c Release
if errorlevel 1 (
  echo.
  echo Build or launch failed. Copy the error messages above for troubleshooting.
  pause
)
endlocal
