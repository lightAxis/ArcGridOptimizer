@echo off
setlocal

:: === 설정(필요시 수정) ===
set PROJECT=ArcGridOptimizer.csproj
set RID=win-x64
set CONFIG=Release
set OUTPUT=publish\win-x64

:: 스크립트 위치 기준으로 이동
cd /d "%~dp0"

:: dotnet 유무 체크
where dotnet >nul 2>&1
if errorlevel 1 (
  echo [ERROR] dotnet SDK가 설치되어 있지 않습니다.
  pause
  exit /b 1
)

:: 출력 폴더 정리 (원하면 주석 처리)
if exist "%OUTPUT%" (
  echo 기존 출력 폴더 정리: "%OUTPUT%"
  rmdir /s /q "%OUTPUT%"
)

echo 퍼블리시 시작...
dotnet publish ".\%PROJECT%" -c %CONFIG% -r %RID% -o "%OUTPUT%" ^
  -p:SelfContained=false ^
  -p:PublishSingleFile=true ^
  -p:PublishReadyToRun=false ^
  -p:CopyOutputSymbolsToPublishDirectory=false ^
  -p:DebugType=none ^
  -p:IncludeNativeLibrariesForSelfExtract=true

if errorlevel 1 (
  echo.
  echo [ERROR] 퍼블리시 실패.
  pause
  exit /b 1
)

echo.
echo 퍼블리시 성공!
echo 출력 경로: "%CD%\%OUTPUT%"
pause
endlocal
