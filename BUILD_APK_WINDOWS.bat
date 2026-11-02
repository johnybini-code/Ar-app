@echo off
setlocal
set UNITY="C:\Program Files\Unity\Hub\Editor\6000.0.62f1\Editor\Unity.exe"
if not exist %UNITY% (
  echo Unity 6000.0.62f1 not found at the standard path.
  echo Install Unity 6.0.62f1 with Android Build Support, OpenJDK, Android SDK and NDK from Unity Hub.
  pause
  exit /b 1
)
%UNITY% -batchmode -quit -projectPath "%~dp0" -executeMethod BiniarosAR.Editor.BiniarosBuildAndroid.BuildAPK -logFile "%~dp0Build\build.log"
if exist "%~dp0Build\BINIAROS_ROMA_AR.apk" (
  echo APK READY: %~dp0Build\BINIAROS_ROMA_AR.apk
) else (
  echo Build did not produce an APK. Check Build\build.log
)
pause
