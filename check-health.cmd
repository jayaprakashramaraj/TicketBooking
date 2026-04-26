@echo off
SETLOCAL EnableDelayedExpansion

echo ========================================================
echo   EasyBooking - Local API Health Checks
echo ========================================================
echo.

set "api_checks=Identity:7000 Catalog:7001 Booking:7002 Payment:7003 Notification:7004 PaymentSimulator:7005"
set "failed=0"

for %%c in (%api_checks%) do (
    for /f "tokens=1,2 delims=:" %%a in ("%%c") do (
        echo Checking %%a API at http://localhost:%%b/health ...
        powershell -NoProfile -ExecutionPolicy Bypass -Command "$uri = 'http://localhost:%%b/health'; try { $response = Invoke-WebRequest -Uri $uri -UseBasicParsing -TimeoutSec 10; $json = $response.Content | ConvertFrom-Json; Write-Host ('  HTTP: ' + $response.StatusCode); Write-Host ('  Status: ' + $json.status); if ($json.checks) { foreach ($check in $json.checks) { $description = if ($check.description) { ' - ' + $check.description } else { '' }; Write-Host ('  - ' + $check.component + ': ' + $check.status + $description) } }; if ($response.StatusCode -lt 200 -or $response.StatusCode -ge 400 -or $json.status -ne 'Healthy') { exit 1 } } catch { Write-Host ('  ERROR: ' + $_.Exception.Message); exit 1 }"
        if errorlevel 1 (
            set "failed=1"
        )
        echo.
    )
)

if "%failed%"=="1" (
    echo One or more health checks failed.
    exit /b 1
)

echo All local API health checks passed.
exit /b 0
