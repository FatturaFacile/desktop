@echo off
cd /d "%~dp0.."
echo running > .claude-build\status.txt
dotnet tool restore > .claude-build\build.log 2>&1
if errorlevel 1 (echo build-failed > .claude-build\status.txt & exit /b 1)
dotnet ef migrations add %1 --project src\FatturaFacile.Infrastructure --startup-project src\FatturaFacile.Wpf --output-dir Persistence\Migrations >> .claude-build\build.log 2>&1
if errorlevel 1 (echo build-failed > .claude-build\status.txt & exit /b 1)
echo ok > .claude-build\status.txt
