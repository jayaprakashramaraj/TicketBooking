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

echo Starting Ticket Booking System Locally (Ports 7000+)...
echo Make sure infrastructure is running: docker-compose -f docker-compose.infra.yml up -d
echo.

:: --- 1. Identity API (Port 7000) ---
echo Launching Identity API...
SET "DB_CONNECTION=Server=%SQL_SERVER%;Database=IdentityDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;Connect Timeout=30;"
start "Identity.API" cmd /k "cd Identity.API\bin\Debug\net10.0 && SET ASPNETCORE_URLS=http://localhost:7000 && SET DB_CONNECTION=%DB_CONNECTION% && SET JWT_SECRET=%JWT_SECRET% && Identity.API.exe"

:: --- 2. Catalog API (Port 7001) ---
echo Launching Catalog API...
SET "MONGO_CONN=mongodb://localhost:27017"
start "Catalog.API" cmd /k "cd Catalog.API\bin\Debug\net10.0 && SET ASPNETCORE_URLS=http://localhost:7001 && SET ConnectionStrings__MongoConnection=%MONGO_CONN% && Catalog.API.exe"

:: --- 3. Booking API (Port 7002) ---
echo Launching Booking API...
SET "DB_CONNECTION=Server=%SQL_SERVER%;Database=BookingDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;Connect Timeout=30;"
start "Booking.API" cmd /k "cd Booking.API\bin\Debug\net10.0 && SET ASPNETCORE_URLS=http://localhost:7002 && SET DB_CONNECTION=%DB_CONNECTION% && SET REDIS_HOST=%REDIS_HOST% && SET RABBITMQ_HOST=%RABBITMQ_HOST% && SET RABBITMQ_PORT=%RABBITMQ_PORT% && Booking.API.exe"

:: --- 4. Payment API (Port 7003) ---
echo Launching Payment API...
SET "DB_CONNECTION=Server=%SQL_SERVER%;Database=PaymentDb;User Id=sa;Password=%DB_PASS%;TrustServerCertificate=True;Encrypt=False;Connect Timeout=30;"
start "Payment.API" cmd /k "cd Payment.API\bin\Debug\net10.0 && SET ASPNETCORE_URLS=http://localhost:7003 && SET DB_CONNECTION=%DB_CONNECTION% && SET RABBITMQ_HOST=%RABBITMQ_HOST% && SET RABBITMQ_PORT=%RABBITMQ_PORT% && Payment.API.exe"

:: --- 5. Notification API (Port 7004) ---
echo Launching Notification API...
start "Notification.API" cmd /k "cd Notification.API\bin\Debug\net10.0 && SET ASPNETCORE_URLS=http://localhost:7004 && SET REDIS_HOST=%REDIS_HOST% && SET RABBITMQ_HOST=%RABBITMQ_HOST% && SET RABBITMQ_PORT=%RABBITMQ_PORT% && Notification.API.exe"

:: --- 6. Frontend ---
echo Launching Frontend...
:: UI will detect port 5173 and use 7000-7004 ports automatically
start "Frontend" cmd /k "cd ticketbooking-ui && npm run dev"

echo.
echo Waiting for services to become healthy...
echo Checking: http://localhost:7002/health

:health_check
:: Wait 2 seconds
timeout /t 2 /nobreak >nul
:: Use PowerShell to check the health endpoint
powershell -Command "$response = try { Invoke-WebRequest -Uri 'http://localhost:7002/health' -UseBasicParsing -TimeoutSec 1 } catch { $null }; if ($response -and $response.Content -like '*Healthy*') { exit 0 } else { exit 1 }"
if %ERRORLEVEL% neq 0 (
    echo | set /p="."
    goto health_check
)

echo.
echo Services are UP and Healthy!
echo Opening UI in Chrome...

:: Open Chrome to the Vite default port
start chrome "http://localhost:5173"

echo Done.
pause
