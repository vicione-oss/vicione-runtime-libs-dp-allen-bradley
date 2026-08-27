@echo off
REM Deploy AllenBradley.Logix to local Vicione installation
REM Usage: deploy-logix-to-local-vicione.cmd <vo-suite-folder> [Release] [runtime-identifier]
REM   Parameter 2: If "Release", builds in Release mode, otherwise Debug (default)
REM   Parameter 3: Runtime identifier (e.g., win-x64, linux-x64, linux-arm64). If not specified, publishes framework-dependent.

if "%~1"=="" (
    echo Error: vo-suite folder not specified
    echo Usage: deploy-logix-to-local-vicione.cmd ^<vo-suite-folder^> [Release] [runtime-identifier]
    exit /b 1
)

set "VO_SUITE_FOLDER=%~1"
set "BUILD_CONFIG=Debug"
set "RUNTIME_ID="

REM Check if second parameter is a runtime identifier or build config
if /I "%~2"=="win-x64" (
    set "RUNTIME_ID=%~2"
) else if /I "%~2"=="linux-x64" (
    set "RUNTIME_ID=%~2"
) else if /I "%~2"=="linux-arm64" (
    set "RUNTIME_ID=%~2"
) else if /I "%~2"=="Release" (
    set "BUILD_CONFIG=Release"
) else if /I "%~2"=="Debug" (
    set "BUILD_CONFIG=Debug"
)

REM If third parameter exists, it's the runtime identifier
if not "%~3"=="" (
    set "RUNTIME_ID=%~3"
)
set "SCRIPT_DIR=%~dp0"
set "SOLUTION_ROOT=%SCRIPT_DIR%.."

REM Read version from VERSION file (handle BOM and whitespace)
set "VERSION_FILE=%SOLUTION_ROOT%\VERSION"
if not exist "%VERSION_FILE%" (
    echo Error: VERSION file not found: %VERSION_FILE%
    exit /b 1
)
REM Use for /f with usebackq to read file - this handles BOM correctly
for /f "usebackq tokens=* delims=" %%a in ("%VERSION_FILE%") do (
    set "VERSION=%%a"
    goto :version_read
)
:version_read
REM Remove any remaining whitespace
for /f "tokens=* delims= " %%a in ("%VERSION%") do set "VERSION=%%a"
echo Using version: %VERSION%
echo.

if "%RUNTIME_ID%"=="" (
    set "SOURCE_FOLDER=%SCRIPT_DIR%..\src\AllenBradley.Logix\bin\%BUILD_CONFIG%\net10.0\publish"
) else (
    set "SOURCE_FOLDER=%SCRIPT_DIR%..\src\AllenBradley.Logix\bin\%BUILD_CONFIG%\net10.0\%RUNTIME_ID%\publish"
)
set "TARGET_FOLDER=%VO_SUITE_FOLDER%\src\Core.OS\bin\Debug\net10.0\Cache_Standalone\ViciOne.Suite.ClusterManagement\Dependencies\ViciOne.Suite.DataPort.AllenBradley.Logix\%VERSION%"

if not exist "%VO_SUITE_FOLDER%" (
    echo Error: vo-suite folder does not exist: %VO_SUITE_FOLDER%
    exit /b 1
)

if "%RUNTIME_ID%"=="" (
    echo Building and publishing AllenBradley.Logix project in %BUILD_CONFIG% mode (framework-dependent^)...
) else (
    echo Building and publishing AllenBradley.Logix project in %BUILD_CONFIG% mode for %RUNTIME_ID%...
)
echo.
pushd "%SOLUTION_ROOT%"
echo Cleaning project...
dotnet clean src\AllenBradley.Logix\AllenBradley.Logix.csproj -c %BUILD_CONFIG%
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Error: Clean failed with error code %ERRORLEVEL%
    popd
    exit /b %ERRORLEVEL%
)
echo.
if "%RUNTIME_ID%"=="" (
    dotnet publish src\AllenBradley.Logix\AllenBradley.Logix.csproj -c %BUILD_CONFIG%
) else (
    dotnet publish src\AllenBradley.Logix\AllenBradley.Logix.csproj -c %BUILD_CONFIG% -r %RUNTIME_ID% --no-self-contained
)
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Error: Build/publish failed with error code %ERRORLEVEL%
    popd
    exit /b %ERRORLEVEL%
)
popd
echo.

if not exist "%SOURCE_FOLDER%" (
    echo Error: Source folder does not exist: %SOURCE_FOLDER%
    echo Publish may have failed.
    exit /b 1
)

if not exist "%TARGET_FOLDER%" (
    echo Target folder does not exist, creating: %TARGET_FOLDER%
    mkdir "%TARGET_FOLDER%" 2>nul
    if not exist "%TARGET_FOLDER%" (
        echo Error: Could not create target folder
        exit /b 1
    )
    echo.
)

echo Copying files from:
echo   %SOURCE_FOLDER%
echo To:
echo   %TARGET_FOLDER%
echo.

xcopy /E /I /Y /Q "%SOURCE_FOLDER%\*" "%TARGET_FOLDER%"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Deployment successful!
) else (
    echo.
    echo Deployment failed with error code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
