#define MyAppVersion GetEnv("VERSION")

[Setup]
AppName=FatturaFacile
AppVersion={#MyAppVersion}
AppPublisher=Davide
DefaultDirName={autopf}\FatturaFacile
DefaultGroupName=FatturaFacile

OutputDir=Output
OutputBaseFilename=FatturaFacile-Setup

Compression=lzma
SolidCompression=yes

[Files]
Source: "..\src\FatturaFacile\bin\Release\net11.0-windows10.0.19041.0\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\FatturaFacile"; Filename: "{app}\FatturaFacile.exe"
Name: "{autodesktop}\FatturaFacile"; Filename: "{app}\FatturaFacile.exe"

[Run]
Filename: "{app}\FatturaFacile.exe"; Description: "Run FatturaFacile"; Flags: nowait postinstall skipifsilent
