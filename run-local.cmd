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
echo [3/5] Launching APIs (7000-7004)...

:: Identity API (Port 7000)
SET "DB_CONNECTION_IDENTITY=Server=%SQL_SERVER%;Database=IdentityDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;"
start "EB_IDENTITY" cmd /c "SET ASPNETCORE_URLS=http://localhost:7000 && SET DB_CONNECTION=%DB_CONNECTION_IDENTITY% && SET JWT_SECRET=%JWT_SECRET% && dotnet run --project src\Identity\Identity.API\Identity.API.csproj --no-build"

:: Catalog API (Port 7001)
SET "MONGO_CONN=mongodb://localhost:27017"
start "EB_CATALOG" cmd /c "SET ASPNETCORE_URLS=http://localhost:7001 && SET ConnectionStrings__MongoConnection=%MONGO_CONN% && dotnet run --project src\Catalog\Catalog.API\Catalog.API.csproj --no-build"

:: Booking API (Port 7002)
SET "DB_CONNECTION_BOOKING=Server=%SQL_SERVER%;Database=BookingDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;"
start "EB_BOOKING" cmd /c "SET ASPNETCORE_URLS=http://localhost:7002 && SET DB_CONNECTION=%DB_CONNECTION_BOOKING% && SET REDIS_HOST=%REDIS_HOST% && SET RABBITMQ_HOST=%RABBITMQ_HOST% && SET RABBITMQ_PORT=%RABBITMQ_PORT% && dotnet run --project src\Booking\Booking.API\Booking.API.csproj --no-build"

:: Payment API (Port 7003)
SET "DB_CONNECTION_PAYMENT=Server=%SQL_SERVER%;Database=PaymentDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;"
start "EB_PAYMENT" cmd /c "SET ASPNETCORE_URLS=http://localhost:7003 && SET DB_CONNECTION=%DB_CONNECTION_PAYMENT% && SET RABBITMQ_HOST=%RABBITMQ_HOST% && SET RABBITMQ_PORT=%RABBITMQ_PORT% && dotnet run --project src\Payment\Payment.API\Payment.API.csproj --no-build"

:: Notification API (Port 7004)
start "EB_NOTIFICATION" cmd /c "SET ASPNETCORE_URLS=http://localhost:7004 && SET REDIS_HOST=%REDIS_HOST% && SET RABBITMQ_HOST=%RABBITMQ_HOST% && SET RABBITMQ_PORT=%RABBITMQ_PORT% && dotnet run --project src\Notification\Notification.API\Notification.API.csproj --no-build"

:: --- 4. Health Checks ---
echo [4/5] Waiting for APIs to be healthy...

set "api_ports=7000 7001 7002 7003 7004"

:wait_loop
set "all_healthy=true"
for %%p in (%api_ports%) do (
    powershell -Command "$response = try { Invoke-WebRequest -Uri 'http://localhost:%%p/health' -UseBasicParsing -TimeoutSec 1 } catch { $null }; if (-not ($response -and $response.Content -like '*Healthy*')) { exit 1 } else { exit 0 }"
    if %ERRORLEVEL% neq 0 (
        set "all_healthy=false"
    )
)

if "%all_healthy%"=="false" (
    echo | set /p="."
    timeout /t 3 /nobreak >nul
    goto wait_loop
)

echo.
echo All APIs are Healthy!

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
    
    :: Kill the API windows and their child dotnet processes
    taskkill /FI "WINDOWTITLE eq EB_*" /F /T >nul 2>&1
    
    :: Kill the UI window (Vite/Node)
    taskkill /FI "WINDOWTITLE eq EB_UI*" /F /T >nul 2>&1
    
    :: Ensure all dotnet and node processes started by the script are gone
    taskkill /IM dotnet.exe /F >nul 2>&1
    taskkill /IM node.exe /F >nul 2>&1
    
    echo.
    echo All services stopped. (Infrastructure remains running in Docker)
    
    echo.
    echo Done.
    exit /b 0
) else (
    goto stop_loop
)
