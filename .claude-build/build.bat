@echo off
cd /d "%~dp0.."
echo running > .claude-build\status.txt
dotnet build FatturaFacile.slnx -nologo -v q -clp:NoSummary > .claude-build\build.log 2>&1
if errorlevel 1 (echo build-failed > .claude-build\status.txt & exit /b 1)
dotnet test tests\FatturaFacile.UnitTests --no-build -nologo -v q > .claude-build\test.log 2>&1
if errorlevel 1 (echo tests-failed > .claude-build\status.txt & exit /b 1)
echo ok > .claude-build\status.txt
