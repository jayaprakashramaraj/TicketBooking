@echo off
SETLOCAL EnableDelayedExpansion

:: --- Configuration ---
SET "DB_PASS=YourStrong@Passw0rd"
SET "REDIS_HOST=127.0.0.1:6380"
SET "RABBITMQ_HOST=127.0.0.1"
SET "RABBITMQ_PORT=5673"
SET "SQL_SERVER=127.0.0.1,1434"
SET "JWT_SECRET=YourSuperSecretKeyThatIsAtLeast32CharsLong!"
SET "ASPNETCORE_ENVIRONMENT=Development"

echo ========================================================
echo   EasyBooking - Local Development Startup
echo ========================================================
echo.

:: --- 1. Start Infrastructure ---
echo [1/5] Starting Infrastructure (Docker)...
docker-compose -f docker-compose.infra.yml up -d
if %ERRORLEVEL% neq 0 (
    echo Error starting infrastructure. Make sure Docker is running.
    pause
    exit /b %ERRORLEVEL%
)

:: --- 2. Build Solution ---
echo [2/5] Building Solution...
dotnet build TicketBooking.slnx
if %ERRORLEVEL% neq 0 (
    echo Build failed. Please check for errors.
    pause
    exit /b %ERRORLEVEL%
)

:: --- 3. Launch APIs ---
echo [3/5] Launching APIs (7000-7005)...

:: Identity API (Port 7000)
SET "DB_CONNECTION_IDENTITY=Server=%SQL_SERVER%;Database=IdentityDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;"
start "EB_IDENTITY" cmd /c "SET ASPNETCORE_URLS=http://localhost:7000&& SET DB_CONNECTION=%DB_CONNECTION_IDENTITY%&& SET JWT_SECRET=%JWT_SECRET%&& dotnet run --project src\Identity\Identity.API\Identity.API.csproj --no-build --no-launch-profile"

:: Catalog API (Port 7001)
SET "MONGO_CONN=mongodb://localhost:27017"
start "EB_CATALOG" cmd /c "SET ASPNETCORE_URLS=http://localhost:7001&& SET MONGO_CONNECTION=%MONGO_CONN%&& dotnet run --project src\Catalog\Catalog.API\Catalog.API.csproj --no-build --no-launch-profile"

:: Booking API (Port 7002)
SET "DB_CONNECTION_BOOKING=Server=%SQL_SERVER%;Database=BookingDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;"
start "EB_BOOKING" cmd /c "SET ASPNETCORE_URLS=http://localhost:7002&& SET DB_CONNECTION=%DB_CONNECTION_BOOKING%&& SET REDIS_HOST=%REDIS_HOST%&& SET RABBITMQ_HOST=%RABBITMQ_HOST%&& SET RABBITMQ_PORT=%RABBITMQ_PORT%&& SET PaymentSimulatorUrl=http://localhost:7005&& SET UIBaseUrl=http://localhost:5173&& dotnet run --project src\Booking\Booking.API\Booking.API.csproj --no-build --no-launch-profile"

:: Payment API (Port 7003)
SET "DB_CONNECTION_PAYMENT=Server=%SQL_SERVER%;Database=PaymentDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;"
start "EB_PAYMENT" cmd /c "SET ASPNETCORE_URLS=http://localhost:7003&& SET DB_CONNECTION=%DB_CONNECTION_PAYMENT%&& SET RABBITMQ_HOST=%RABBITMQ_HOST%&& SET RABBITMQ_PORT=%RABBITMQ_PORT%&& dotnet run --project src\Payment\Payment.API\Payment.API.csproj --no-build --no-launch-profile"

:: Notification API (Port 7004)
start "EB_NOTIFICATION" cmd /c "SET ASPNETCORE_URLS=http://localhost:7004&& SET REDIS_HOST=%REDIS_HOST%&& SET RABBITMQ_HOST=%RABBITMQ_HOST%&& SET RABBITMQ_PORT=%RABBITMQ_PORT%&& dotnet run --project src\Notification\Notification.API\Notification.API.csproj --no-build --no-launch-profile"

:: Payment Simulator (Port 7005)
start "EB_SIMULATOR" cmd /c "SET ASPNETCORE_URLS=http://localhost:7005&& SET RABBITMQ_HOST=%RABBITMQ_HOST%&& SET RABBITMQ_PORT=%RABBITMQ_PORT%&& dotnet run --project src\PaymentSimulator\PaymentSimulator.API\PaymentSimulator.API.csproj --no-build --no-launch-profile"

:: --- 4. Health Checks ---
echo [4/5] Waiting for APIs to be ready...

set "api_checks=7000:/health 7001:/health 7002:/health 7003:/health 7004:/health 7005:/health"

:wait_loop
set "all_healthy=true"
for %%c in (%api_checks%) do (
    for /f "tokens=1,2 delims=:" %%a in ("%%c") do (
        powershell -Command "$response = try { Invoke-WebRequest -Uri 'http://localhost:%%a%%b' -UseBasicParsing -TimeoutSec 2 } catch { $null }; if (-not ($response -and [int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 400)) { exit 1 } else { exit 0 }"
    )
    if errorlevel 1 (
        set "all_healthy=false"
    )
)

if "%all_healthy%"=="false" (
    echo | set /p="."
    timeout /t 3 /nobreak >nul
    goto wait_loop
)

echo.
echo All APIs are ready!

:: --- 5. Launch UI ---
echo [5/5] Launching UI and opening Chrome...
start "EB_UI" cmd /c "cd ticketbooking-ui && npm run dev"

:: Wait for Vite to start up a bit
timeout /t 5 /nobreak >nul

:: Open Chrome
start chrome "http://localhost:5173"

echo.
echo ========================================================
echo   EasyBooking is now running!
echo   TYPE 'Q' AND PRESS ENTER TO STOP ALL SERVICES
echo ========================================================
echo.

:stop_loop
set /p "input=Command (Q to quit): "
if /I "%input%"=="Q" (
    echo Stopping all EasyBooking services...
    
    :: Kill processes listening on local app ports, plus any leftover project processes.
    :: Window-title based taskkill is unreliable because child process titles can change.
    powershell -NoProfile -ExecutionPolicy Bypass -Command "$ports = 7000,7001,7002,7003,7004,7005,5173; Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue | Where-Object { $ports -contains $_.LocalPort } | Select-Object -ExpandProperty OwningProcess -Unique | Where-Object { $_ -gt 0 } | ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }; Get-CimInstance Win32_Process | Where-Object { $_.Name -in @('dotnet.exe','node.exe','cmd.exe') -and $_.CommandLine -match 'TicketBooking|ticketbooking-ui|Identity\.API|Catalog\.API|Booking\.API|Payment\.API|Notification\.API|PaymentSimulator\.API|vite' } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }"
    
    echo.
    echo All services stopped. (Infrastructure remains running in Docker)
    
    echo.
    echo Done.
    exit /b 0
) else (
    goto stop_loop
)
