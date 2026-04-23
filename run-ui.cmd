:: --- Health Checks ---
echo Waiting for APIs to be healthy...

set "api_ports=7000 7001 7002 7003 7004"

:wait_loop
set "all_healthy=true"

for %%p in (%api_ports%) do (
    powershell -Command "$response = try { Invoke-WebRequest -Uri 'http://localhost:%%p/health' -UseBasicParsing -TimeoutSec 1 } catch { $null }; if (-not ($response -and $response.Content -like '*Healthy*')) { exit 1 } else { exit 0 }"
    
    if !ERRORLEVEL! neq 0 (
        set "all_healthy=false"
    )
)

if "!all_healthy!"=="false" (
    echo Waiting...
    timeout /t 3 /nobreak >nul
    goto wait_loop
)

echo All APIs are Healthy!

:: --- Start UI ONLY after health is confirmed ---
echo Starting UI...

start "EB_UI" cmd /c "cd ticketbooking-ui && npm run dev"

timeout /t 5 /nobreak >nul
start chrome "http://localhost:5173"