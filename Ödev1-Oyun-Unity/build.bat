@echo off
echo ================================================
echo  EG-Kart - Unity Build + Installer Olusturucu
echo ================================================
echo.

:: Unity Editor yolu (2022 LTS)
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe"
set PROJECT_PATH=%~dp0
set BUILD_PATH=%~dp0build\EG-Kart
set INNO_PATH="C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

echo [1/3] Unity ile Windows Build olusturuluyor...
echo Proje: %PROJECT_PATH%
echo Cikti: %BUILD_PATH%
echo.

%UNITY_PATH% ^
  -quit ^
  -batchmode ^
  -nographics ^
  -projectPath "%PROJECT_PATH%" ^
  -buildWindowsPlayer "%BUILD_PATH%\EG-Kart.exe" ^
  -logFile "%PROJECT_PATH%\build_log.txt"

if %ERRORLEVEL% NEQ 0 (
  echo [HATA] Unity build basarisiz! Log: build_log.txt
  pause
  exit /b 1
)

echo [OK] Unity build tamamlandi!
echo.
echo [2/3] Inno Setup ile installer olusturuluyor...

%INNO_PATH% "%PROJECT_PATH%installer\setup.iss"

if %ERRORLEVEL% NEQ 0 (
  echo [HATA] Inno Setup basarisiz!
  echo Inno Setup kurulu mu? https://jrsoftware.org/isinfo.php
  pause
  exit /b 1
)

echo [OK] Installer olusturuldu: installer\output\EGKart_Setup_v1.0.exe
echo.
echo [3/3] Tum islemler tamamlandi!
echo.
echo Olusturulan dosyalar:
echo   - build\EG-Kart\EG-Kart.exe  (oyun)
echo   - installer\output\EGKart_Setup_v1.0.exe  (installer)
echo.
pause
