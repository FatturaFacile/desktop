@echo off
cd /d "%~dp0.."
set ConnectionStrings__DefaultConnection=Data Source=%~dp0ui-test.sqlite
set Security__FieldEncryptionKeyPath=%~dp0ui-test.key
start "" "src\FatturaFacile.Wpf\bin\Debug\net11.0-windows10.0.19041.0\FatturaFacile.Wpf.exe"
