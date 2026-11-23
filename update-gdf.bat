@echo off
REM Haalt de nieuwste versie van GDF op en zet die in de map lib van dit project.
REM Je eigen code en je map Data blijven ongemoeid. Maak toch even een backup voor de zekerheid.

if not exist "%~dp0lib\update-gdf.ps1" (
    echo Kan lib\update-gdf.ps1 niet vinden. Staat update-gdf.bat wel in de projectmap?
    pause
    exit /b 1
)

REM let op: "%~dp0" eindigt op een \ ; met de punt erachter blijft het pad geldig binnen de aanhalingstekens
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0lib\update-gdf.ps1" -ProjectMap "%~dp0." %*
echo.
pause
