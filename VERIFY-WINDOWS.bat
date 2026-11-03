@echo off
setlocal
cd /d "%~dp0"
echo.
echo ========== NEON BREACH 1.1 / WINDOWS VERIFICATION ==========
echo.
where dotnet >nul 2>nul
if errorlevel 1 (
 echo ERROR: Install the .NET 8 SDK first:
 echo https://dotnet.microsoft.com/download/dotnet/8.0
 pause
 exit /b 1
)
echo [1/3] Restoring NuGet packages...
dotnet restore NEON_BREACH.csproj
if errorlevel 1 goto :failed
echo [2/3] Building Release configuration...
dotnet build NEON_BREACH.csproj -c Release --no-restore
if errorlevel 1 goto :failed
echo [3/3] Running automated logic and asset smoke tests...
dotnet run --project NEON_BREACH.csproj -c Release --no-build -- --self-test
if errorlevel 1 goto :failed
echo.
echo SUCCESS! Build and self-tests passed. Launch RUN-WINDOWS.bat to play.
pause
exit /b 0
:failed
echo.
echo VERIFICATION FAILED: copy the compiler output or failed checks above.
pause
exit /b 1
