@echo off
cd /d "%~dp0"
where dotnet >nul 2>&1
if errorlevel 1 (
 echo Instale o SDK .NET 10 em https://dotnet.microsoft.com/download/dotnet/10.0
 pause
 exit /b 1
)
dotnet restore src/EstoqueFacil.Api
if errorlevel 1 (
 pause
 exit /b 1
)
dotnet run --project src/EstoqueFacil.Api --launch-profile http
pause
