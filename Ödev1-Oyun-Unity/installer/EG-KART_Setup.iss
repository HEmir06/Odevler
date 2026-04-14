[Setup]
AppName=EG-KART
AppVersion=1.0
DefaultDirName={autopf}\EG-KART
DefaultGroupName=EG-KART
UninstallDisplayIcon={app}\EG-KART.exe

[Files]
Source: "..\Build\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\EG-KART"; Filename: "{app}\EG-KART.exe"; WorkingDir: "{app}"
Name: "{commondesktop}\EG-KART"; Filename: "{app}\EG-KART.exe"; WorkingDir: "{app}"

[Code]
function InitializeSetup(): Boolean;
var InstalledMark: string;
begin
  Result := True;
  if RegQueryStringValue(HKEY_CURRENT_USER, 'Software\EG-KART', 'InstalledSignature', InstalledMark) then
  begin
    MsgBox('Bu uygulama bu sisteme daha önce kurulmuştur ve ödev kuralları gereği tekrar kurulamaz.', mbError, MB_OK);
    Result := False; 
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    RegWriteStringValue(HKEY_CURRENT_USER, 'Software\EG-KART', 'InstalledSignature', 'YES');
  end;
end;
