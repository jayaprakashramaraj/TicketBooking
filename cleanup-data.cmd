@echo off
SETLOCAL EnableDelayedExpansion

echo ========================================================
echo   EasyBooking - Data Cleanup Script
echo ========================================================
echo WARNING: This will PERMANENTLY DELETE all data in:
echo   - SQL Server (Identity, Booking, Payment databases)
echo   - MongoDB (Catalog data)
echo   - Redis (Cache/Sessions)
echo   - RabbitMQ (Queues/Exchanges)
echo.

set /p "confirm=Are you sure you want to proceed? (y/n): "
if /I "%confirm%" neq "y" (
    echo Cleanup cancelled.
    exit /b 0
)

echo.
echo [1/3] Stopping infrastructure and removing volumes...
:: -v removes named volumes declared in the volumes section of the Compose file
docker-compose -f docker-compose.infra.yml down -v

if %ERRORLEVEL% neq 0 (
    echo Error during cleanup. Make sure Docker is running and you have permissions.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo [2/3] Cleaning up any dangling volumes (Optional)...
:: This catches any persistent data that might have escaped the previous command
docker volume prune -f --filter "label=com.docker.compose.project=ticketbooking" >nul 2>&1

echo.
echo [3/3] Restarting infrastructure...
docker-compose -f docker-compose.infra.yml up -d

if %ERRORLEVEL% neq 0 (
    echo Error restarting infrastructure.
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================================
echo   Cleanup Complete! 
echo   Infrastructure is now fresh and running.
echo ========================================================
echo.
pause
