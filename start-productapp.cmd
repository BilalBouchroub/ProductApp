@echo off
setlocal

set "PROJECT_ROOT=%~dp0"

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERREUR] Le SDK .NET est introuvable. Installez-le puis relancez ce fichier.
    pause
    exit /b 1
)

where npm.cmd >nul 2>&1
if errorlevel 1 (
    echo [ERREUR] Node.js et npm sont introuvables. Installez-les puis relancez ce fichier.
    pause
    exit /b 1
)

if not exist "%PROJECT_ROOT%frontend\node_modules" (
    echo Installation initiale des dependances frontend...
    call npm.cmd install --prefix "%PROJECT_ROOT%frontend"
    if errorlevel 1 (
        echo [ERREUR] L'installation des dependances frontend a echoue.
        pause
        exit /b 1
    )
)

for /f "delims=" %%K in ('powershell.exe -NoProfile -Command "[Guid]::NewGuid().ToString('N') + [Guid]::NewGuid().ToString('N')"') do set "Jwt__SigningKey=%%K"

echo Demarrage de ProductApp...
echo API      : http://localhost:5220
echo Frontend : http://localhost:5173
echo.
echo Fermez les deux nouvelles fenetres pour arreter le projet.

start "ProductApp API" /D "%PROJECT_ROOT%backend" cmd.exe /k dotnet run --project "src\ProductApp.Api\ProductApp.Api.csproj" --launch-profile http
start "ProductApp Frontend" /D "%PROJECT_ROOT%frontend" cmd.exe /k npm.cmd run dev -- --open

endlocal
