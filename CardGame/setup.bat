@echo off
echo CardGame Setup for Friend
echo ========================
echo.

echo This will set up everything to run the game server.
echo.

echo Step 1: Checking .NET 8...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo .
    echo .NET 8 SDK is required!
    echo Please download from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo .
    echo After installing, run this script again.
    pause
    exit /b 1
)

echo ✓ .NET 8 is installed

echo.
echo Step 2: Creating configuration...
cd src\CardGame.Server
if not exist "appsettings.Development.json" (
    if exist "appsettings.Example.json" (
        copy "appsettings.Example.json" "appsettings.Development.json"
        echo ✓ Created configuration file
    ) else (
        echo ✗ Configuration files missing!
        pause
        exit /b 1
    )
)

echo.
echo Step 3: Installing packages...
dotnet restore

echo.
echo ✅ Setup complete!
echo.
echo To start the server:
echo 1. Run start.bat in this folder
echo 2. Or run: dotnet run
echo.
echo The server will start on http://localhost:5000
echo.
pause