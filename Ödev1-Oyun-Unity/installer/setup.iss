; ============================================================
; EG-Kart Inno Setup Installer Scripti
; Tek seferlik kurulum kilidi: Windows Registry HKLM tabanlı
; ============================================================

#define MyAppName "EG-Kart"
#define MyAppVersion "1.0"
#define MyAppPublisher "EG Studio"
#define MyAppExeName "EG-Kart.exe"
#define MyRegPath "SOFTWARE\EGOyun\EGKart"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=.\output
OutputBaseFilename=EGKart_Setup_v{#MyAppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=admin
WizardStyle=modern
SetupIconFile=assets\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

; UI Renkleri (Mor tema)
WizardImageFile=assets\wizard_banner.bmp
WizardSmallImageFile=assets\wizard_small.bmp

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Ek ikonlar:";

[Files]
; Oyun dosyaları (Unity build çıktısı)
Source: "build\EG-Kart\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{#MyAppName}'ı Kaldır"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

; ============================================================
; TEK SEFERLİK KURULUM KİLİDİ — KRİTİK BÖLÜM
; Bu kontrol sayesinde:
;   - Kurulum ilk kez yapılınca Registry'e Installed=1 yazar
;   - İkinci kurulum denemesinde BLOKE eder
;   - Yazılım kaldırılsa da kilit kalır (Uninstall bölümünde silinmez!)
; ============================================================

[Code]

// ---- KURULUM BAŞLARKEN KONTROL ----
function InitializeSetup(): Boolean;
var
  installedValue: String;
begin
  // Registry'de kilit anahtarı var mı?
  if RegQueryStringValue(HKLM, '{#MyRegPath}', 'Installed', installedValue) then
  begin
    if installedValue = '1' then
    begin
      MsgBox(
        'EG-Kart bu bilgisayara daha önce kurulmuştur.' + #13#10 +
        #13#10 +
        'Bu kurulum paketi yalnızca bir kez kullanılabilir.' + #13#10 +
        'Kurulum işlemi sonlandırılıyor.',
        mbError,
        MB_OK
      );
      Result := False;  // Kurulumu tamamen iptal et
      Exit;
    end;
  end;
  Result := True;  // İlk kurulum — devam et
end;

// ---- KURULUM TAMAMLANINCA KİLİT YAZ ----
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
  begin
    // Registry'e kalıcı kilit yaz
    RegWriteStringValue(HKLM, '{#MyRegPath}', 'Installed', '1');
    RegWriteStringValue(HKLM, '{#MyRegPath}', 'InstallDate',
      GetDateTimeString('yyyy-mm-dd', '-', ':'));
    RegWriteStringValue(HKLM, '{#MyRegPath}', 'Version', '{#MyAppVersion}');
  end;
end;

// ---- KALDIRMA ESNASINDA KİLİDİ KORU ----
// NOT: Registry anahtarı [UninstallDelete] veya [Registry] bölümünde
// hiçbir zaman silinmez. CurUninstallStepChanged'de de silme işlemi yok.
// Bu sayede kaldırma sonrası bile kilit kalır.
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // BİLEREK BOŞ BIRAKILDI:
  // Kilit anahtarı (SOFTWARE\EGOyun\EGKart) kaldırma sırasında SİLİNMEZ.
  // Yazılım kaldırılsa bile aynı installer tekrar çalıştırılamaz.
  if CurUninstallStep = usPostUninstall then
  begin
    MsgBox(
      'EG-Kart kaldırıldı.' + #13#10 +
      'Bu kurulum paketi bir kez kullanılmıştır ve tekrar çalıştırılamaz.',
      mbInformation,
      MB_OK
    );
  end;
end;
