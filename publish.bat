@echo off
setlocal enabledelayedexpansion

:: Check if static icon data is present
echo Checking if static icon data is present...
set isPresent=T
if not exist RatScanner\Data\correlation.json set isPresent=F
if not exist RatScanner\Data\name\ set isPresent=F
if "%isPresent%"=="F" (
    echo Static icon data missing^^!
    echo Did you copy it to the Data folder when cloning the repository?
    exit /b
)

:: Remove old publish folder
echo Removing old publish folder...
rmdir /s /q publish

:: Publish RatScanner project
echo Publishing RatScanner project...
dotnet publish RatScanner/RatScanner.csproj -c Release -o publish --runtime win-x64 --self-contained false

:: Copy data folder to publish directory
echo Copying data folder to publish directory...
xcopy "RatScanner\Data\*" "publish\Data\" /y /e /s /q

:: Delete unused pdb
echo Deleting unused pdb...
del publish\OpenCvSharpExtern.pdb

:: Delete reference to pdb
echo Deleting reference to pdb...
set /A REMOVE_COUNT=2
set "SEARCH_STR=OpenCvSharpExtern.pdb"
set "SRC_FILE=publish\RatScanner.deps.json"

set /A SKIP_COUNT=0
for /F "skip=2 delims=[] tokens=1,*" %%I in ('find /v /n "" "%SRC_FILE%"') do (
    if !SKIP_COUNT! EQU 0 (
        set SRC_LINE=%%J
        if defined SRC_LINE (
            if "!SRC_LINE:%SEARCH_STR%=!" == "!SRC_LINE!" (
                echo.!SRC_LINE! >> publish\RatScanner.deps.json.tmp
            ) else (
                set /A SKIP_COUNT=%REMOVE_COUNT%
            )
        ) else (
            rem SRC_LINE is empty
            echo. >> publish\RatScanner.deps.json.tmp
        )
    ) else (
        set /A SKIP_COUNT-=1
    )
)

del publish\RatScanner.deps.json

rename publish\RatScanner.deps.json.tmp RatScanner.deps.json

:: Build updater
:: echo Building updater...
:: MSBuild Updater/Updater.vcxproj /p:Configuration=Release /p:Platform=x64

:: Copy updater binary to publish directory
:: echo Copying updater binary into publish directory
:: xcopy "Updater\build\x64\Release\Updater.exe" "publish\" /y /e /s /q

:: Finalize publish
echo Done^^!
echo Run the published version to make sure there were no errors while publishing^^!
pause